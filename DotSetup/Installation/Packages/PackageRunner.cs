// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Threading;

namespace DotSetup
{
    internal class PackageRunner
    {
        private readonly InstallationPackage installationPackage;
        Thread runnerThread = null;
        Mutex msiRunMutex = null;
        public bool waitForRunner;
        private readonly object terminationLock = new object();
        internal const int ERROR_INSTALL_ALREADY_RUNNING = 1618;
        public PackageRunner(InstallationPackage installationPackage)
        {
            this.installationPackage = installationPackage;
        }

        internal void Run(string runName, string runParam = "")
        {
            waitForRunner = false;
            runnerThread = new Thread(() => {
                lock (terminationLock)
                {
                    try
                    {
                        Process p = new System.Diagnostics.Process();
                        p.StartInfo.FileName = runName;
                        p.StartInfo.Arguments = runParam;
                        int msiTimeout = installationPackage.msiTimeout;

#if DEBUG
                        Logger.GetLogger().Info("Running " + runName + " " + runParam);
#endif
                        // The default value of UseShellExecute is specific to a platform, so we determine the value for all cases
                        if (Path.GetExtension(runName).ToLower() != ".exe" && Path.GetExtension(runName).ToLower() != ".msi")
                            p.StartInfo.UseShellExecute = true;
                        else
                            p.StartInfo.UseShellExecute = false;

                        try
                        {
                            if (Path.GetExtension(runName).ToLower() == ".msi")
                            {
                                if (!WaitForMsi(runName, msiTimeout))
                                {
                                    throw new Win32Exception(ERROR_INSTALL_ALREADY_RUNNING);
                                }
                            }
                            installationPackage.HandleRunStart();
                            p.EnableRaisingEvents = true;
                            p.Exited += ProcessEnded;
                            p.Start();
                        }
                        catch (Win32Exception ex)
                        {
                            installationPackage.runErrorCode = ex.NativeErrorCode;
                            throw;
                        }
                    }
                    catch (Exception ex)
                    {
                        installationPackage.errorMessage = $"Running {runName} {runParam} - {ex.Message}";
                        installationPackage.OnInstallFailed(ErrorConsts.ERR_RUN_GENERAL, installationPackage.errorMessage);
                    }

                    if (!installationPackage.waitForIt)
                        installationPackage.HandleRunEnd();
                }
            });
            runnerThread.Start();
        }

        private void ProcessEnded(object sender, EventArgs e)
        {
            if (sender is Process process)
            {
                installationPackage.runExitCode = process.ExitCode;
                if (installationPackage.waitForIt)
                    installationPackage.HandleRunEnd();
            }
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
