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
        private readonly InstallationPackage _installationPackage;
        private Thread _runnerThread = null;
        private Mutex _msiRunMutex = null;
        private readonly bool _waitForRunner;

        public bool BitsEnabled { get; private set; }
        private readonly object terminationLock = new object();
        internal const int ERROR_INSTALL_ALREADY_RUNNING = 1618;
        public PackageRunner(InstallationPackage installationPackage)
        {
            _installationPackage = installationPackage;
            BitsEnabled = false;
            _waitForRunner = false;
        }

        internal void Run(string runName, string runParam = "")
        {
            _runnerThread = new Thread(() =>
            {
                lock (terminationLock)
                {
                    try
                    {
                        try
                        {
                            Process p = null;

#if DEBUG
                            Logger.GetLogger().Info($"Running {runName} {runParam}");
#endif

                            if (_installationPackage.RunWithBits && !FileUtils.IsRunnableFile(runName))
                            {
#if DEBUG
                                Logger.GetLogger().Info($"{runName} sets to run with BITS but it's not an executable file. BITS is not going to be used");
#endif
                                _installationPackage.RunWithBits = false;
                            }

                            if (_installationPackage.RunWithBits)
                            {
                                BitsEnabled = true;
                            }
                            else
                            {
                                p = new Process();
                                p.StartInfo.FileName = runName;
                                p.StartInfo.Arguments = runParam;
                                p.StartInfo.UseShellExecute = !FileUtils.IsRunnableFileExtension(runName);
                            }

                            if (Path.GetExtension(runName).ToLower() == ".msi")
                            {
                                int msiTimeout = _installationPackage.msiTimeout;
                                if (!WaitForMsi(runName, msiTimeout))
                                {
                                    throw new Win32Exception(ERROR_INSTALL_ALREADY_RUNNING);
                                }
                            }

                            _installationPackage.HandleRunStart();

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
                            if (_installationPackage.onRunWithBits.Set() && BitsEnabled)
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
                                        _installationPackage.runExitCode = process.ExitCode;
                                        if (_installationPackage.waitForIt)
                                            _installationPackage.HandleRunEnd();
                                    }
                                };

                                if (!BitsEnabled)
                                    p.Start();
                            }
                            else
                            {
                                throw new Exception($"Process not found: {Path.GetFileName(runName)}");
                            }

                        }
                        catch (Win32Exception ex)
                        {
                            _installationPackage.runErrorCode = ex.NativeErrorCode;
                            throw;
                        }
                        finally
                        {
                            BitsEnabled = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _installationPackage.ErrorMessage = $"Running {runName} {runParam} - {ex.Message}";
                        _installationPackage.OnInstallFailed(ErrorConsts.ERR_RUN_GENERAL, _installationPackage.ErrorMessage);
                    }

                    if (!_installationPackage.waitForIt)
                        _installationPackage.HandleRunEnd();
                }
            });
            _runnerThread.Start();
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
                _msiRunMutex = Mutex.OpenExisting(installerServiceMutexName, MutexRights.Synchronize);
                isMsiFree = _msiRunMutex.WaitOne(maxWaitTime, false);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                // Mutex doesn't exist, do nothing
                isMsiFree = true;
            }
            finally
            {
                if ((_msiRunMutex != null) && !isMsiFree)
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
            if (_waitForRunner)
            {
                lock (terminationLock)
                {

                }
            }
            else if (_runnerThread != null)
            {
                _runnerThread.Join(1000);
            }
        }
    }
}
