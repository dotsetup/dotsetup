// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.Windows.Forms;
using DotSetup.UILayouts.ProductLayouts;

namespace DotSetup.Installation.WinForm
{
    public class FormPageUtils
    {
        public static class PageName
        {
            public const string Splash = "splashPage",
                                Loading = "loadingPage",
                                Welcome = "welcomePage",
                                License = "licensePage",
                                Optional = "optionalPage",
                                Progress = "progressPage",
                                Finish = "finishPage",
                                DownloadError = "downloadErrorPage",
                                ConfigurationError = "configurationErrorPage";
        }
    }

    public interface IFormPageBinder
    {
        Form GetFormPage(string pageName, FrmParent frmParent);
        void SetProductLayouts(ProductLayoutManager productLayoutManager);
    }

}
