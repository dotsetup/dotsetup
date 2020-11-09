// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Windows.Forms;

namespace DotSetup
{
    public class DotSetupManager
    {
        public static class EventName
        {
            public const string OnProgress = "OnProgress",
								OnFatalError = "OnFatalError",
                                OnLoadDynamiConfig = "OnLoadDynamiConfig",
                                OnRemoteConfigResponse = "OnRemoteConfigResponse",
                                OnFirstDownloadEnd = "OnFirstDownloadEnd",
                                OnLoading = "OnLoading";
        }



        internal ConfigLoader configLoader;
        internal PackageManager packageManager;
        private SingleInstance singleInstance;

        public DotSetupManager(string[] args, Boolean ExternalParent = false)
        {
            configLoader = new ConfigLoader(args);
            ValidateSingleInstance();
            packageManager = new PackageManager();
			EventManager.GetManager().AddEvent(EventName.OnRemoteConfigResponse, HandleServerResponse);
        }

        internal void HandlePageLoad(string formName)
        {
            if (formName == FormPageUtils.PageName.Progress)
                packageManager.DoRun();
        }

        internal bool HandleServerResponse(object sender, EventArgs e)
        {
            configLoader.UpdateInstallerManager(this);
            return true;
        }

        public void FinalizeInstaller(bool runOnClose)
        {
#if DEBUG
            Logger.GetLogger().Info("Finalizing application...");
#endif
            packageManager.HandleInstallerQuit(runOnClose);
#if DEBUG
            Logger.GetLogger().Info("Finalizing complete");
#endif
        }

        private void ValidateSingleInstance()
        {
            singleInstance = new SingleInstance(ConfigParser.GetConfig().GetConfigValue(ConfigConsts.PRODUCT_TITLE, "SINGLEINSTANCEANDNAMEDPIPE"));
            if (!singleInstance.IsApplicationFirstInstance())
            {
                singleInstance.NamedPipeClientSendCmd(@"SHOW");
#if DEBUG
                Logger.GetLogger().Info("Other instance of the installer is already running, sending \"show\" message and quitting");
#endif
                Environment.Exit(0);
            }

            singleInstance.OnPipeCmdEvent += HandlePipeCmd;
        }

        internal void HandlePipeCmd(PipeCmdEventArgs e)
        {
            switch (e.Data)
            {
                case "SHOW":
#if DEBUG
                    Logger.GetLogger().Info("Received \"show\" command from other instance");
#endif
                    foreach (Form frm in Application.OpenForms)
                    {
                        frm.PerformSafely(() => frm.WindowState = FormWindowState.Normal);
                        frm.PerformSafely(() => frm.Activate());
                    }
                    break;
                default:
#if DEBUG
                    Logger.GetLogger().Warning("Recieved unknown command via pipe from other instance: " + e.Data);
#endif
                    break;
            }
        }

    }
}
