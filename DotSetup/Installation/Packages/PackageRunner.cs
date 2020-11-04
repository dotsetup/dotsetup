// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.ComponentModel;
using System.IO;
using System.Security.AccessControl;
using System.Threading;

namespace DotSetup
{
    internal class PackageRunner
    {
        private InstallationPackage installationPackage;
        Thread runnerThread = null;
        Mutex msiRunMutex = null;
        bool waitForRunner;
        private readonly object terminationLock = new object();
        internal const int ERROR_INSTALL_ALREADY_RUNNING = 1618;
        public PackageRunner(InstallationPackage installationPackage)
        {
            this.installationPackage = installationPackage;
        }

        internal void Run(string runName, string runParam = "", bool waitForIt = false)
        {
            waitForRunner = waitForIt;
            runnerThread = new Thread(() => StartRunnerProc(runName, runParam, waitForIt));
            runnerThread.Start();
        }

        internal bool WaitForMsi(string runName, int maxWaitTime)
        {
            const string installerServiceMutexName = "Global\\_MSIExecute";
            bool isMsiFree = false;
#if DEBUG
            Logger.GetLogger().Info("Timeout  for running msi - " + maxWaitTime);
#endif
            try
            {
                msiRunMutex = Mutex.OpenExisting(installerServiceMutexName, MutexRights.Synchronize);
                isMsiFree = msiRunMutex.WaitOne(maxWaitTime, false);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                // Mutex doesn't exist, do nothing
                isMsiFree = true;
            }
            finally
            {
                if ((msiRunMutex != null) && !isMsiFree)
                {
#if DEBUG
                    Logger.GetLogger().Info("Timeout expired for running msi - " + runName);
#endif
                }
            }
            return isMsiFree;
        }

        internal void StartRunnerProc(string runName, string runParam, bool waitForIt)
        {
            lock (terminationLock)
            {
                try
                {
                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo.FileName = runName;
                    p.StartInfo.Arguments = runParam;
                    int msiTimeout = installationPackage.msiTimeout;

#if DEBUG
                    Logger.GetLogger().Info("Running " + runName + " " + runParam);
#endif
                    if (Path.GetExtension(runName).ToLower() != ".exe" && Path.GetExtension(runName).ToLower() != ".msi")
                        p.StartInfo.UseShellExecute = false;

                    if (Path.GetExtension(runName).ToLower() == ".msi")
                    {
                        if (!WaitForMsi(runName, msiTimeout))
                        {
                            throw new Win32Exception(ERROR_INSTALL_ALREADY_RUNNING);
                        }
                    }

                    p.Start();
                    if (waitForIt)
                        while (!p.WaitForExit(5000)) ;

                }
                catch (Exception ex)
                {
                    installationPackage.OnInstallFailed(ErrorConsts.ERR_RUN_GENERAL, "Running " + runName + " " + runParam + " - " + ex.ToString());
                }
                installationPackage.HandleRunFinished();
            }
        }

        internal void Terminate()
        {
            if (waitForRunner)
            {
                lock (terminationLock)
                {

                }
            }
            else if (runnerThread != null)
            {
                runnerThread.Join(1000);
            }
        }
    }
}
