// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.Threading;
using System.Windows.Forms;

namespace DotSetup
{
    public class ProductLayoutManager
    {
        private readonly List<ProductLayout> _productLayouts;
        private readonly List<ProductSettings> _productSettings;
        private CountdownEvent _preparingResources;
        private ProductLayout _currntLayout;
        private int _productIndex = 0, _internalSkippedProducts = 0;
        private readonly PackageManager _packageManager;
        private Panel _pnlLayout;
        private OptLayout _optLayout;
        internal Action<string, int> OnLayoutShown, OnLayoutError;

        public int RemainingProducts => (_productLayouts != null) ? _productLayouts.Count - _productIndex : 0;

        public ProductLayoutManager(PackageManager packageManager)
        {
            _packageManager = packageManager;
            _productLayouts = new List<ProductLayout>();
            _productSettings = new List<ProductSettings>();
            _preparingResources = new CountdownEvent(0);
        }

        public int GetIndex(string pkgName) => _productSettings.FindIndex(prod => prod.Name == pkgName);

        public void AddProductSettings(ProductSettings prodSettings)
        {
            if (!prodSettings.IsOptional)
                return;

            if (_waitForProductsSettingsControlsResources)
            {
                _preparingResources = new CountdownEvent(1);
                _waitForProductsSettingsControlsResources = false;
            } 
            else if (_preparingResources.IsSet)
                _preparingResources.Reset(1);
            else
                _preparingResources.AddCount();
            
            prodSettings.ControlsLayouts.PrepareResources(_preparingResources);
            _productSettings.Add(prodSettings);
        }

        private bool _waitForProductsSettingsControlsResources = false;
        public bool WaitForProductsSettingsControlsResources(int timeOutMS)
        {
            if (_waitForProductsSettingsControlsResources)
                return true;

#if DEBUG
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
#endif            
            bool res = _preparingResources.Wait(timeOutMS);
            if (res)
            {
                _waitForProductsSettingsControlsResources = true;
                _preparingResources.Dispose();
            }
#if DEBUG
            stopWatch.Stop();
            Logger.GetLogger().Info($"WaitForProductsSettingsControlsResources takes {stopWatch.ElapsedMilliseconds} milliseconds");
#endif
            return res;
        }

        public bool AddProductLayouts()
        {
            bool isSuccess = true;
            try
            {
                foreach (ProductSettings prodSettings in _productSettings)
                {
                    ProductLayout productLayout = new ProductLayout(prodSettings.Name, prodSettings.LayoutName, prodSettings.ControlsLayouts);
                    if (string.IsNullOrEmpty(productLayout.errorMsg))
                    {
                        _productLayouts.Add(productLayout);
                    }
                    else
                    {
                        _packageManager.DiscardPackge(prodSettings.Name, productLayout.errorMsg);
                        isSuccess = false;
                    }
                }
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Error($"AddProductLayouts failed, error: {e.Message}");
#endif
                isSuccess = false;
            }
            return isSuccess;
        }

        public void SetPnlLayout(Panel pnlLayout)
        {
            _pnlLayout = pnlLayout;

            if (pnlLayout == null || !HasProducts() || !NextLayout())
            {
#if DEBUG
                Logger.GetLogger().Warning("No optional products to show - The optional page should not show");
#endif
            }
        }

        public bool IsPnlLayoutSet() => (_pnlLayout != null) && (_currntLayout != null);

        public bool HasProducts() => _productLayouts != null && _productLayouts.Count > _internalSkippedProducts;

        public bool NextLayout()
        {
            if (_productLayouts != null && _productIndex < _productLayouts.Count)
            {
                _currntLayout = _productLayouts[_productIndex];
                
                if (!_currntLayout.controlLayout.ResourcesReady)
                {
                    InstallationPackage package = _packageManager.GetPackageByName(_currntLayout.productName);
                    if (package != null)
                    {
                        package.ErrorMessage = $"missing resource for control layout";
                        OnLayoutError(_currntLayout.productName, _productIndex);
                        package?.OnInstallFailed(ErrorConsts.ERR_PKG_MISSING_LAYOUT_RESOURCES, package.ErrorMessage);
                    }
                    _productIndex++;
                    _internalSkippedProducts++;
                    _currntLayout.controlLayout.StopWaitingForResources();
                    _currntLayout = null;
                    return NextLayout();
                }
                
                if (_pnlLayout.Controls.Count > 0)
                    _pnlLayout.Resize -= ((ProductControl)_pnlLayout.Controls[0]).HandleChanges;

                _pnlLayout.Controls.Clear();
                _pnlLayout.Controls.Add(_currntLayout.productLayout);
                _currntLayout.productLayout.HandleChanges();
                _pnlLayout.Resize += ((ProductControl)_pnlLayout.Controls[0]).HandleChanges;
                _optLayout = new OptLayout(_pnlLayout, 4);
                OnLayoutShown(_currntLayout.productName, _productIndex);
            }
            _productIndex++;
            return _productLayouts == null || _productIndex <= _productLayouts.Count;
        }

        public bool AcceptClicked()
        {
            bool hasNext = false;
            if (!IsPnlLayoutSet())
                return hasNext;
            if (_optLayout.opt == OptLayout.OptType.IN && _optLayout.optIn != null && !_optLayout.optIn.Checked)
            {
                hasNext = DeclineClicked();
            }
            else if (_optLayout.opt == OptLayout.OptType.SMART)
            {
                hasNext = true;
                if (!_optLayout.smOptInY.Checked && !_optLayout.smOptInN.Checked)
                {
                    if (!_optLayout.smOptShown)
                        _optLayout.ShowSmOptInOverlay();
                    else
                        _optLayout.BlinkSmartOptin();
                }
                else if (_optLayout.smOptInY.Checked)
                {
                    _optLayout.RemoveDarken();
                    _packageManager.ConfirmPackage(_currntLayout.productName);
                    hasNext = NextLayout();
                }
                else if (_optLayout.smOptInN.Checked)
                {
                    hasNext = DeclineClicked();
                }
            }
            else
            {
                _packageManager.ConfirmPackage(_currntLayout.productName);
                hasNext = NextLayout();
            }
            return hasNext;
        }

        public bool DeclineClicked()
        {
            bool hasNext = false;
            if (IsPnlLayoutSet())
            {
                _optLayout.RemoveDarken();
                _packageManager.DeclinePackage(_currntLayout.productName);
                hasNext = NextLayout();
            }
            return hasNext;
        }

        public bool SkipAllClicked()
        {
            if (IsPnlLayoutSet())
            {
                _optLayout.RemoveDarken();
                _packageManager.SkipAll(_currntLayout.productName);
            }
            //all products were skipped so no more layouts...
            return false;
        }

        public bool IsSkipAllButtonNeeded()
        {
            return _packageManager.GetOptionalsCount() > 1;
        }
    }
}
