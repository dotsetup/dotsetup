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
        private readonly List<ProductLayout> productLayouts;
        private readonly List<ProductSettings> productSettings;        
        private ProductLayout currntLayout;
        private int productIndex;
        private readonly PackageManager packageManager;
        private Panel pnlLayout;
        private OptLayout optLayout;

        public bool HasNext { get; private set; }

        public ProductLayoutManager(PackageManager packageManager)
        {
            this.packageManager = packageManager;
            productLayouts = new List<ProductLayout>();
            productSettings = new List<ProductSettings>();
        }

        public void AddProductSettings(ProductSettings prodSettings)
        {
            if (prodSettings.IsOptional && !(prodSettings.ControlsLayouts is null))
               productSettings.Add(prodSettings);
        }

        public void AddProductLayouts()
        {
            foreach(ProductSettings prodSettings in productSettings)
            {
                ProductLayout productLayout = new ProductLayout(prodSettings.Name, prodSettings.LayoutName, prodSettings.ControlsLayouts);
                productLayout.productLayout.VisibleChanged += new EventHandler(UserControl_OnShow);
                productLayouts.Add(productLayout);
            }
        }

        protected void UserControl_OnShow(object sender, EventArgs e)
        {
            
        }

        public void SetPnlLayout(Panel pnlLayout)
        {
            this.pnlLayout = pnlLayout;

            if (pnlLayout == null || !HasProducts())
            {
#if DEBUG
                Logger.GetLogger().Warning("No optional products to show - The optional page should not show");
#endif
            }
            else
            {
                productIndex = 0;
                NextLayout();
            }
        }

        public bool IsPnlLayoutSet()
        {
            return (pnlLayout != null) && (currntLayout != null);
        }

        public bool HasProducts()
        {
            return productLayouts != null && productLayouts.Count > 0;
        }

        public bool NextLayout()
        {
            if (productLayouts != null && productIndex < productLayouts.Count)
            {
                currntLayout = productLayouts[productIndex];
                pnlLayout.Controls.Clear();
                pnlLayout.Controls.Add(currntLayout.productLayout);
                optLayout = new OptLayout(pnlLayout, 4);
            }
            productIndex++;
            HasNext = (productLayouts == null || productIndex <= productLayouts.Count);
            return HasNext;
        }

        public bool AcceptClicked()
        {
            bool hasNext = false;
            if (!IsPnlLayoutSet())
                return hasNext;
            if (optLayout.opt == OptLayout.OptType.IN && optLayout.optIn != null && !optLayout.optIn.Checked)
            {
                hasNext = DeclineClicked();
            }
            else if (optLayout.opt == OptLayout.OptType.SMART)
            {
                hasNext = true;
                if (!optLayout.smOptInY.Checked && !optLayout.smOptInN.Checked)
                {
                    if (!optLayout.smOptShown)
                        optLayout.ShowSmOptInOverlay();
                    else
                        optLayout.BlinkSmartOptin();
                }
                else if (optLayout.smOptInY.Checked)
                {
                    optLayout.RemoveDarken();
                    hasNext = NextLayout();
                }
                else if (optLayout.smOptInN.Checked)
                {
                    hasNext = DeclineClicked();
                }
            }
            else
            {
                hasNext = NextLayout();
            }
            return hasNext;
        }

        public bool DeclineClicked()
        {
            bool hasNext = false;
            if (IsPnlLayoutSet())
            {
                optLayout.RemoveDarken();
                packageManager.DeclinePackge(currntLayout.productName);
                hasNext = NextLayout();
            }
            return hasNext;
        }

        public bool SkipAllClicked()
        {
            bool hasNext = false;
            if (IsPnlLayoutSet())
            {
                optLayout.RemoveDarken();
                packageManager.SkipAll();
            }
            return hasNext;
        }

        public bool IsSkipAllButtonNeeded()
        {
            return packageManager.GetOptionalsCount() > 1;
        }
    }
}
