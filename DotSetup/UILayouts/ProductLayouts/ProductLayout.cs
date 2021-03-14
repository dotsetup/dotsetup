// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

namespace DotSetup
{

    public class ProductLayout
    {
        internal string productName, errorMsg;
        internal ProductControl productLayout;
        internal readonly ControlsLayout controlLayout;

        public ProductLayout(string productName, string layoutName, ControlsLayout controlLayout)
        {
            errorMsg = "";
            try
            {
                this.productName = productName;
                this.controlLayout = controlLayout;

                if (layoutName == typeof(ProductLayout2).Name)
                    productLayout = new ProductLayout2(controlLayout);
                else if (layoutName == typeof(ProductLayout4).Name)
                    productLayout = new ProductLayout4(controlLayout);
                else if (layoutName == typeof(ProductLayout5).Name)
                    productLayout = new ProductLayout5(controlLayout);
                else if (layoutName == typeof(ProductLayout6).Name)
                    productLayout = new ProductLayout6(controlLayout);
            }
            catch (System.Exception e)
            {
                errorMsg = "Initializing product layout of " + productName + " failed with error: " + e.Message;
#if DEBUG
                Logger.GetLogger().Error(errorMsg);
#endif
            }
            finally
            {
                if (string.IsNullOrEmpty(errorMsg) && productLayout == null)
                    errorMsg = "No product with name " + productName;
            }
        }
    }
}
