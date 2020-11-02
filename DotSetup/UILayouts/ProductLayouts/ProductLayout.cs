// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.Windows.Forms;

namespace DotSetup
{

    public class ProductLayout
    {
        internal string productName;
        internal UserControl productLayout;

        public ProductLayout(string productName, string layoutName, ControlsLayout ControlLayout)
        {
            try
            {
                this.productName = productName;
                if (layoutName == typeof(ProductLayout1).Name)
                    productLayout = new ProductLayout1(ControlLayout);
                if (layoutName == typeof(ProductLayout2).Name)
                    productLayout = new ProductLayout2(ControlLayout);
                if (layoutName == typeof(ProductLayout3).Name)
                    productLayout = new ProductLayout3(ControlLayout);
                if (layoutName == typeof(ProductLayout4).Name)
                    productLayout = new ProductLayout4(ControlLayout);
                if (layoutName == typeof(ProductLayout5).Name)
                    productLayout = new ProductLayout5(ControlLayout);
                if (layoutName == typeof(ProductLayout6).Name)
                    productLayout = new ProductLayout6(ControlLayout);
            }
#if DEBUG
            catch (System.Exception e)
            {
                Logger.GetLogger().Error("Initializing product layout of " + productName + " failed with error: " + e.Message);
            }
#endif
            finally
            {
            }
        }
    }
}