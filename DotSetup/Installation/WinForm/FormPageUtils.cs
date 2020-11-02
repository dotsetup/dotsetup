// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.Windows.Forms;

namespace DotSetup
{
    public class FormPageUtils
    {
        public static class PageName
        {
            public const string Welcome = "welcomePage", License = "licensePage",
                Optional = "optionalPage", Progress = "progressPage", Finish = "finishPage", Splash = "splashPage",
                Error = "errorPage";
        }
    }

    public interface IFormPageBinder
    {
        Form GetFormPage(string pageName, frmParent frmParent);
        void SetProductLayouts(ProductLayoutManager productLayoutManager);
        InstallerEventHandler GetLoadingCallBack(frmParent frmParent);
    }

}
