// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DotSetup
{
    class DotSetupSDK
    {
        private static DotSetupManager dotSetupManager;
        internal static Panel pnlLayout;
        private static DockingHelper pnlLayoutDockingHelper;

        public static int Init(string accountID)
        {
            //TODO: make validation for accountID
            if (accountID != "<expected key>")
                return 0;
            InitApplication();
            string[] args = new string[] { "/log" };
            dotSetupManager = DotSetupManager.GetManager();
            dotSetupManager.InitInstaller(args,null);
            return 1;
        }
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        public static int SetWindow(IntPtr wnd, int clientX, int clientY, int clientWidth, int clientHeight)
        {
            if (pnlLayout == null && (wnd != IntPtr.Zero) && (dotSetupManager != null))
            {
                pnlLayout = new Panel { Left = clientX, Top = clientY, Width = clientWidth, Height = clientHeight, Visible = false };
                dotSetupManager.packageManager.productLayoutManager.SetPnlLayout(pnlLayout);
                SetParent(pnlLayout.Handle, wnd);
                pnlLayoutDockingHelper = new DockingHelper(wnd, pnlLayout);
            }
            return (pnlLayout != null) ? 1 : 0;
        }

        public static int Show()
        {
            if (pnlLayout == null)
                return 0;
            pnlLayout.Visible = true;
            pnlLayoutDockingHelper?.Subscribe();
            return 1;
        }

        public static int Hide()
        {
            if (pnlLayout == null)
                return 0;
            pnlLayout.Visible = false;
            return 1;
        }

        public static int Accept()
        {
            bool hasNext = false;
            if (dotSetupManager != null)
                hasNext = dotSetupManager.packageManager.productLayoutManager.AcceptClicked();
            if (!hasNext)
                OnNoNext();
            return (hasNext) ? 1 : 0;
        }

        public static int Decline()
        {
            bool hasNext = false;
            if (dotSetupManager != null)
                hasNext = dotSetupManager.packageManager.productLayoutManager.DeclineClicked();
            if (!hasNext)
                OnNoNext();
            return (hasNext) ? 1 : 0;
        }

        public static int Install()
        {
            int pkgStartedCount = 0;
            if (dotSetupManager != null)
                pkgStartedCount = dotSetupManager.packageManager.Activate();
            return (pkgStartedCount > 0) ? 1 : 0;
        }

        public static int GetProgress()
        {
            int progress = 0;
            if (dotSetupManager != null)
                progress = dotSetupManager.packageManager.GetCurrentProgress();
            return progress;
        }

        public static int InstallationSuccess()
        {
            return 1;
        }

        public static int Finalize()
        {
            int finalizeSuccsess = 0;
            if (dotSetupManager != null)
            {
                dotSetupManager.FinalizeInstaller(false);
                finalizeSuccsess = 1;
            }
            return finalizeSuccsess;
        }

        private static void OnNoNext()
        {
            pnlLayout.Visible = false;
            pnlLayoutDockingHelper?.Unsubscribe();
            pnlLayout.Dispose();
            pnlLayout = null;
        }

        internal static void InitApplication()
        {
            if (ResourcesUtils.libraryAssembly != null) return;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);
            ResourcesUtils.wrapperAssembly = Assembly.GetExecutingAssembly();
            ResourcesUtils.libraryAssembly = Assembly.GetExecutingAssembly();
        }

        internal static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
#if DEBUG
            DotSetup.Logger.GetLogger().Error(ex.Message);
#endif
        }

    }
}
