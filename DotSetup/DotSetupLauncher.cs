// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Reflection;
using System.Windows.Forms;
using DotSetup.Infrastructure;
using DotSetup.Installation.Configuration;
using DotSetup.Installation.Packages;

namespace DotSetup
{
    public class DotSetupLauncher
    {
        internal ConfigLoader configLoader;
        private SingleInstance singleInstance;

        internal static DotSetupLauncher Instance { get; set; } = null;
        public PackageManager PkgManager { get; set; }

        public DotSetupLauncher()
        {
            CommunicationUtils.EnableHighestTlsVersion();
        }

        public static DotSetupLauncher GetLauncher()
        {
            if (Instance == null)
                Instance = new DotSetupLauncher();
            return Instance;
        }

        public virtual void InitInstaller(string[] args, Assembly assembly)
        {
            ResourcesUtils.wrapperAssembly = assembly;
            InitConfig(args);
            PkgManager = new PackageManager();
            PkgManager.ParseProducts();
        }

        internal void InitConfig(string[] args)
        {
            configLoader = new ConfigLoader(args);
            if (Instance == null)
                Instance = this;
            ValidateSingleInstance();
        }

        public int Activate()
        {
            return PkgManager.Activate();
        }

        public virtual void FinalizeInstaller(bool runOnClose)
        {
#if DEBUG
            Logger.GetLogger().Info("Finalizing application...");
#endif
            PkgManager.HandleInstallerQuit(runOnClose);
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
