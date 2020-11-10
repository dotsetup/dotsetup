// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DotSetup
{
#pragma warning disable IDE1006 // Naming Styles
    public partial class frmParent : Form
#pragma warning restore IDE1006 // Naming Styles
    {
        readonly Dictionary<string, Form> formsDictionary = new Dictionary<string, Form>();
        readonly List<String> pageNameList;
        Form currentForm;
        int currntPageIndex;
        readonly DotSetupManager dotSetupManager;
        readonly InstallerEventHandler onLoadingHandler = null;

        public frmParent(string[] args, Assembly assembly, IFormPageBinder pageBinder)
        {
            InitializeComponent();
            ResourcesUtils.wrapperAssembly = assembly;
            ResourcesUtils.libraryAssembly = Assembly.GetExecutingAssembly();
            onLoadingHandler = pageBinder.GetLoadingCallBack(this);

            if (onLoadingHandler != null)
            {
                EventManager.GetManager().AddEvent(DotSetupManager.EventName.OnLoading, onLoadingHandler);
                LoadingEventArgs loadingEventArgs = new LoadingEventArgs(true);
                EventManager.GetManager().DispatchEvent(DotSetupManager.EventName.OnLoading, this, loadingEventArgs);
            }

            dotSetupManager = new DotSetupManager(args);
            formsDictionary = dotSetupManager.configLoader.FormsDictionary(this, pageBinder);
            pageNameList = dotSetupManager.configLoader.PagesNames();
            currntPageIndex = 0;
            dotSetupManager.configLoader.UpdateParentForm(this);

            pageBinder.SetProductLayouts(dotSetupManager.packageManager.productLayoutManager);

            SetTaskbarIcon();
        }

        public void LoadFormName(string formName)
        {
#if DEBUG
            Logger.GetLogger().Info("Loading FormName " + formName);
#endif

            dotSetupManager.HandlePageLoad(formName);

            if (formsDictionary.ContainsKey(formName))
            {
                currentForm = formsDictionary[formName];
                currentForm.TopLevel = false;
                currentForm.AutoScroll = true;
                currentForm.Dock = DockStyle.Fill;
                pnlContent.Controls.Clear();
                pnlContent.Controls.Add(currentForm);

                currentForm.Show();
                currentForm.Activate();

                if (onLoadingHandler != null)
                {
                    LoadingEventArgs loadingEventArgs = new LoadingEventArgs(false);
                    EventManager.GetManager().DispatchEvent(DotSetupManager.EventName.OnLoading, this, loadingEventArgs);
                }

                ConfigParser.GetConfig().SetStringValue(SessionDataConsts.ROOT + SessionDataConsts.CURRENT_FORM, formName);
            }
        }

        public void LoadNextFrom()
        {
            if (currentForm != null)
            {
                int nextPageIndex = pageNameList.FindIndex(x => currentForm.Name.ToLower().Contains(x.ToLower()));
#if DEBUG
                if (nextPageIndex == -1)
                    Logger.GetLogger().Error("No Form called " + currentForm.Name + " found in [" + String.Join(" ", pageNameList) + " ]");
#endif
                currntPageIndex = nextPageIndex + 1;
            }

            if (pageNameList.Count > currntPageIndex)
                LoadFormName(pageNameList[currntPageIndex]);
        }

#pragma warning disable IDE1006 // Naming Styles
        public void frmParent_Load(object sender, EventArgs e)
#pragma warning restore IDE1006 // Naming Styles
        {
            if (formsDictionary != null)
                LoadNextFrom();
            else
                HandleInstallerQuit(false);
        }

        public void HandleInstallerQuit(bool runOnClose)
        {
            dotSetupManager.FinalizeInstaller(runOnClose);
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;
        [DllImport("User32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public void Draggable_OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        const int WS_MINIMIZEBOX = 0x20000;
        const int CS_DBLCLKS = 0x8;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= WS_MINIMIZEBOX;
                cp.ClassStyle |= CS_DBLCLKS;
                return cp;
            }
        }

        public void SetTaskbarIcon()
        {
            if (ResourcesUtils.EmbeddedResourceExists(ResourcesUtils.wrapperAssembly, ".ico"))
                Icon = new System.Drawing.Icon(ResourcesUtils.GetEmbeddedResourceStream(ResourcesUtils.wrapperAssembly, ".ico"));
            else
            {
#if DEBUG
                Logger.GetLogger().Warning("Set at least one icon as embedded resource so that it will be found by " + this.GetType().Name);
#endif
            }
        }
    }
}
