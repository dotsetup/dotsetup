// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Reflection;
using System.Windows.Forms;
using DotSetup.Infrastructure;

namespace DotSetup
{
    public class DotSetupManager
    {
        public static class EventName
        {
            public const string OnProgress = "OnProgress",
                                OnFirstDownloadEnd = "OnFirstDownloadEnd",
                                OnFatalError = "OnFatalError";
        }

        internal ConfigLoader configLoader;
        internal PackageManager packageManager;
        private SingleInstance singleInstance;

        private static DotSetupManager instance = null;
        public static DotSetupManager GetManager()
        {
            if (instance == null)
                instance = new DotSetupManager();
            return instance;
        }

        public DotSetupManager()
        {
            CommunicationUtils.EnableHighestTlsVersion();
        }

        public void InitInstaller(string[] args, Assembly assembly)
        {
            ResourcesUtils.wrapperAssembly = assembly;
            ResourcesUtils.libraryAssembly = Assembly.GetExecutingAssembly();
            configLoader = new ConfigLoader(args);
            ValidateSingleInstance();
            packageManager = new PackageManager();
        }

        public void Activate()
        {
            packageManager.Activate();
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
