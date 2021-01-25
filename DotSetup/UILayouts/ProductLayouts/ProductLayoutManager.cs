// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DotSetup
{
    public class ProductLayoutManager
    {
        private readonly List<ProductLayout> _productLayouts;
        private readonly List<ProductSettings> _productSettings;
        private ProductLayout _currntLayout;
        private int _productIndex = 0;
        private readonly PackageManager _packageManager;
        private Panel _pnlLayout;
        private OptLayout _optLayout;
        internal Action<string, int> OnLayoutShown;

        public bool HasNext { get; private set; }

        public int RemainingProducts => (_productLayouts != null) ? _productLayouts.Count - _productIndex : 0;

        public ProductLayoutManager(PackageManager packageManager)
        {
            _packageManager = packageManager;
            _productLayouts = new List<ProductLayout>();
            _productSettings = new List<ProductSettings>();
        }

        public int GetIndex(string pkgName) => _productSettings.FindIndex(prod => prod.Name == pkgName);

        public void AddProductSettings(ProductSettings prodSettings)
        {
            if (!prodSettings.IsOptional)
                return;
            _productSettings.Add(prodSettings);
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

            if (pnlLayout == null || !HasProducts())
            {
#if DEBUG
                Logger.GetLogger().Warning("No optional products to show - The optional page should not show");
#endif
            }
            else
            {
                _productIndex = 0;
                NextLayout();
            }
        }

        public bool IsPnlLayoutSet() => (_pnlLayout != null) && (_currntLayout != null);

        public bool HasProducts() => _productLayouts != null && _productLayouts.Count > 0;

        public bool NextLayout()
        {
            if (_productLayouts != null && _productIndex < _productLayouts.Count)
            {
                _currntLayout = _productLayouts[_productIndex];
                if (_pnlLayout.Controls.Count > 0)
                    _pnlLayout.Resize -= ((ProductControl)_pnlLayout.Controls[0]).HandleChanges;

                _pnlLayout.Controls.Clear();
                _pnlLayout.Controls.Add(_currntLayout.productLayout);
                _pnlLayout.Resize += ((ProductControl)_pnlLayout.Controls[0]).HandleChanges;
                _optLayout = new OptLayout(_pnlLayout, 4);
                OnLayoutShown(_currntLayout.productName, _productIndex);
            }
            _productIndex++;
            HasNext = (_productLayouts == null || _productIndex <= _productLayouts.Count);
            return HasNext;
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
