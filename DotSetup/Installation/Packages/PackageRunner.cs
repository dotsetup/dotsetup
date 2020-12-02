// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using DotSetup.Infrastructure;

namespace DotSetup
{
    internal class PackageRunner
    {
        private readonly InstallationPackage installationPackage;
        private Thread runnerThread = null;
        private Mutex msiRunMutex = null;
        public bool waitForRunner;
        private bool _runWithBits;
        public bool RunWithBits => _runWithBits;
        private readonly object terminationLock = new object();
        internal const int ERROR_INSTALL_ALREADY_RUNNING = 1618;
        public PackageRunner(InstallationPackage installationPackage)
        {
            this.installationPackage = installationPackage;
            _runWithBits = false;
            waitForRunner = false;
        }

        internal void Run(string runName, string runParam = "")
        {
            runnerThread = new Thread(() =>
            {
                lock (terminationLock)
                {
                    try
                    {
                        try
                        {
                            Process p = null;

#if DEBUG
                            Logger.GetLogger().Info("Running " + runName + " " + runParam);
#endif
                            if (!FileUtils.IsRunnableFileExtension(runName))
                            {
                                p = new System.Diagnostics.Process();
                                p.StartInfo.UseShellExecute = true;
                                p.StartInfo.FileName = runName;
                                p.StartInfo.Arguments = runParam;
                            }
                            else
                            {
                                _runWithBits = true;
                            }


                            if (Path.GetExtension(runName).ToLower() == ".msi")
                            {
                                int msiTimeout = installationPackage.msiTimeout;
                                if (!WaitForMsi(runName, msiTimeout))
                                {
                                    throw new Win32Exception(ERROR_INSTALL_ALREADY_RUNNING);
                                }
                            }

                            installationPackage.HandleRunStart();

                            Process[] preRunningProcesses = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(runName));
                            int preRunningProcessesCount = preRunningProcesses.Length;
                            foreach (Process preProcess in preRunningProcesses)
                            {
                                preProcess.EnableRaisingEvents = true;
                                preProcess.Exited += new EventHandler(delegate (object sender, EventArgs e)
                                {
                                    preRunningProcessesCount--;
                                });
                            }

                            //release the JobTransferred lock
                            if (installationPackage.onRunWithBits.Set() && _runWithBits)
                            {
                                Process[] processByName = new Process[] { };
                                Stopwatch timer = new Stopwatch();
                                timer.Start();
                                do
                                {
                                    processByName = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(runName));
                                    Thread.Sleep(100);
                                } while (processByName.Length <= preRunningProcessesCount && timer.Elapsed.TotalSeconds < 10);
                                timer.Stop();

                                if (processByName.Length > preRunningProcessesCount)
                                {
                                    foreach (Process candidate in processByName)
                                    {
                                        if (preRunningProcesses.All(current => current.Id != candidate.Id) &&
                                            candidate.ProcessName != candidate.Parent().ProcessName)
                                        {
                                            p = candidate;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (p != null)
                            {
                                p.EnableRaisingEvents = true;
                                p.Exited += (object sender, EventArgs e) =>
                                {
                                    if (sender is Process process)
                                    {
                                        installationPackage.runExitCode = process.ExitCode;
                                        if (installationPackage.waitForIt)
                                            installationPackage.HandleRunEnd();
                                    }
                                };

                                if (!_runWithBits)
                                    p.Start();
                            }
                            else
                            {
                                throw new Exception($"Process not found: {Path.GetFileName(runName)}");
                            }

                        }
                        catch (Win32Exception ex)
                        {
                            installationPackage.runErrorCode = ex.NativeErrorCode;
                            throw;
                        }
                        finally
                        {
                            _runWithBits = false;
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
