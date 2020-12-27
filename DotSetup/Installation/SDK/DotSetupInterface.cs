// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.Diagnostics;

namespace DotSetup
{
    public class DotSetupInterface
    {
        public DotSetupInterface()
        {
        }

        public void Load(string[] args, System.Reflection.Assembly assembly, string language)
        {
            DotSetupManager.GetManager().InitInstaller(args, assembly);
            ConfigParser.GetConfig().SetClientSelectedLocale(language);
        }

        public bool SetWindow(System.Windows.Forms.Panel pnlLayout)
        {
            bool isSuccess = DotSetupManager.GetManager().packageManager.productLayoutManager.AddProductLayouts();
            if (isSuccess)
                DotSetupManager.GetManager().packageManager.productLayoutManager.SetPnlLayout(pnlLayout);
            return isSuccess;
        }

        public bool HasProducts()
        {
            return DotSetupManager.GetManager().packageManager.productLayoutManager.HasProducts();
        }

        public string GetStringFromXml(string Xpath, string defaultValue = "")
        {
            return ConfigParser.GetConfig().GetStringValue(Xpath, defaultValue);
        }

        public bool Accept()
        {
            return DotSetupManager.GetManager().packageManager.productLayoutManager.AcceptClicked();
        }

        public bool Decline()
        {
            return DotSetupManager.GetManager().packageManager.productLayoutManager.DeclineClicked();
        }

        public bool SkipAll()
        {
            return DotSetupManager.GetManager().packageManager.productLayoutManager.SkipAllClicked();
        }

        public int Activate()
        {
            return DotSetupManager.GetManager().Activate();
        }

        public int GetProgress()
        {
            if (!DotSetupManager.GetManager().packageManager.Started())
                return 0;
            if (ProgressBarUpdater.currentState == ProgressEventArgs.State.Done)
                return 100;
            return (int)System.Math.Round(ProgressBarUpdater.currentProgress * 0.98);
        }

        public void Unload(bool exitGracefully)
        {
            if (exitGracefully)
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                while (timer.Elapsed.TotalSeconds < 180 && DotSetupManager.GetManager().packageManager.Activated && (GetProgress() >= 0) && (GetProgress() < 100))
                    System.Threading.Thread.Sleep(1000);
                timer.Stop();
            }
            DotSetupManager.GetManager().FinalizeInstaller(false);
        }
    }
}
