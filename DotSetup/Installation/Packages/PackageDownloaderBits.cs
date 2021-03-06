﻿// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Globalization;
using System.IO;
using System.Timers;
using DotSetup.Infrastructure;
using DotSetup.Installation.Packages;
using BITS = BITSReference1_5;
using BITS5 = BITSReference5_0;

public enum BitsNotifyFlags : uint
{
    JOB_TRANSFERRED = 0x0001,
    JOB_ERROR = 0x0002,
    DISABLE = 0x0004,
    JOB_MODIFICATION = 0x0008,
    FILE_TRANSFERRED = 0x0010,
    FILE_RANGES_TRANSFERRED = 0x0020,
}

namespace DotSetup.Installation.Packages
{
    internal class PackageDownloaderBits : PackageDownloader, BITS.IBackgroundCopyCallback
    {
        public static string Method = "bits";
        private string _downloadFileName;
        private BITS.IBackgroundCopyJob _job;
        private readonly Timer _aTimer;
        private int _totalPercentage = 0;
        private int _timeCounter = 0;
        private const int MAX_TIMEOUT_MS = 30_000;
        private const int RETRY_DELAY_SECONDS = 5;
        private const int TIMER_INTERVAL_MS = 500;

        public PackageDownloaderBits(InstallationPackage installationPackage) : base(installationPackage)
        {
            _aTimer = new Timer
            {
                Interval = TIMER_INTERVAL_MS,
                Enabled = false
            };
            _aTimer.Elapsed += new ElapsedEventHandler(TimerElapsed);
        }

        public override bool Download(string downloadLink, string outFilePath)
        {
            if (!UserUtils.IsSessionUser())
            {
                installationPackage.HandleDownloadError("User must be the sessions user to download with Bits.");
                return false;
            }

            // Already on an MTA thread, so just go for it
            if (System.Threading.Thread.CurrentThread.GetApartmentState() == System.Threading.ApartmentState.MTA)
                return DownloadMTA(downloadLink, outFilePath);

#if DEBUG
            Logger.GetLogger().Info("Download was called in STA apartment state. Moving to MTA apartment so BITS com object will be marshaled in a thread safe mode");
#endif
            // Local variable to hold the caught exception until the caller can rethrow
            Exception downloadException = null;
            bool downloadMTARes = false;

            System.Threading.ThreadStart mtaThreadStart = new System.Threading.ThreadStart(() =>
            {
                try
                {
                    downloadMTARes = DownloadMTA(downloadLink, outFilePath);
                }
                catch (Exception ex)
                {
                    downloadException = ex;
                }
            });

            System.Threading.Thread mtaThread = new System.Threading.Thread(mtaThreadStart);
            mtaThread.SetApartmentState(System.Threading.ApartmentState.MTA);
            mtaThread.Start();
            mtaThread.Join();

            if (downloadException != null) throw downloadException;

            return downloadMTARes;
        }

        private bool DownloadMTA(string downloadLink, string outFilePath)
        {
            BITS.BackgroundCopyManager1_5 mgr;
            try
            {
                mgr = new BITS.BackgroundCopyManager1_5();
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                installationPackage.HandleDownloadError("BackgroundCopyManager is not initialized - " + e.Message);
                return false;
            }

            if (mgr == null)
            {
                installationPackage.HandleDownloadError("BackgroundCopyManager is not initialized");
                return false;
            }

            try
            {
                // A single user can create a maximum of 60 jobs at one time...
                mgr.CreateJob("DotSetup Installer", BITS.BG_JOB_TYPE.BG_JOB_TYPE_DOWNLOAD, out _, out _job);
                SetJobProperties(_job);

                _downloadFileName = outFilePath;
                _job.AddFile(downloadLink, outFilePath);

                if (_job is BITS5.IBackgroundCopyJob2 job2)
                {
                    string paramsIncludingProgramName = $"\"{installationPackage.RunFileName}\" {installationPackage.RunParams}";
                    job2.SetNotifyCmdLine(installationPackage.RunFileName, paramsIncludingProgramName);
                }

                //Activating events for job.
                _job.SetNotifyFlags(
                  (uint)BitsNotifyFlags.JOB_TRANSFERRED
                  + (uint)BitsNotifyFlags.JOB_ERROR);
                _job.SetNotifyInterface(this);

                _job.Resume();  //starting the job
            }
            catch (Exception e)
            {
                installationPackage.HandleDownloadError(e.Message);
                CancelJob();
                return false;
            }

            _aTimer.Start();

            return true;
        }

        private void SetJobProperties(BITS.IBackgroundCopyJob job)
        {
            //TODO  - BITS_JOB_TRANSFER_POLICY = BITS_JOB_TRANSFER_POLICY_ALWAYS

            if (job is BITS5.IBackgroundCopyJob5 job5)
            {
                var value = new BITS5.BITS_JOB_PROPERTY_VALUE
                {
                    Enable = 1
                };
                job5.SetProperty(BITS5.BITS_JOB_PROPERTY_ID.BITS_JOB_PROPERTY_DYNAMIC_CONTENT, value);

                var value_high_perf = new BITS5.BITS_JOB_PROPERTY_VALUE
                {
                    Enable = 0
                };
                job5.SetProperty(BITS5.BITS_JOB_PROPERTY_ID.BITS_JOB_PROPERTY_HIGH_PERFORMANCE, value_high_perf);

            }

            job.SetPriority(BITS.BG_JOB_PRIORITY.BG_JOB_PRIORITY_FOREGROUND);
            job.SetMinimumRetryDelay(RETRY_DELAY_SECONDS);
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            TimerCalled();
        }

        private void TimerCalled()
        {
            try
            {
                _job.GetState(out BITS.BG_JOB_STATE state);
                if (state == BITS.BG_JOB_STATE.BG_JOB_STATE_CONNECTING ||
                    state == BITS.BG_JOB_STATE.BG_JOB_STATE_TRANSIENT_ERROR)
                {
                    //job in a state that that is connecting or in no connection error
                    if (_timeCounter >= MAX_TIMEOUT_MS) //30 seconds of timeout 
                    {
                        string errdesc = string.Empty;
                        if (state == BITS.BG_JOB_STATE.BG_JOB_STATE_TRANSIENT_ERROR)
                        {
                            _job.GetError(out BITS.IBackgroundCopyError pError);
                            pError.GetErrorDescription((uint)CultureInfo.GetCultureInfo("en-US").LCID, out errdesc);
                        }

                        CancelJob();
                        _aTimer.Stop();
                        installationPackage.HandleDownloadError("No Internet connection" + (string.IsNullOrEmpty(errdesc) ? "" : $", error: {errdesc}"));
                    }
                    else
                    {
                        if (state == BITS.BG_JOB_STATE.BG_JOB_STATE_TRANSIENT_ERROR)
                            _job.Resume();

                        _timeCounter += TIMER_INTERVAL_MS;
                    }
                }
                else
                {
                    //job in a state that that can continue to download and progress
                    _timeCounter = 0;
                    _job.GetProgress(out BITS._BG_JOB_PROGRESS progress);

                    if (progress.BytesTotal != ulong.MaxValue)
                    {
                        _totalPercentage = (int)((double)progress.BytesTransferred / progress.BytesTotal * 100);
                        installationPackage.SetDownloadProgress(_totalPercentage, (long)progress.BytesTransferred, (long)progress.BytesTotal);
                    }
                    installationPackage.HandleProgress(installationPackage);
                }
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Error($"error on TimerCalled: {e.Message}, \ntrace:\n {e.StackTrace}");
#endif
            }
        }

        // remove this attribute in order to debug this notification
        [System.Diagnostics.DebuggerHidden()]
        public void JobTransferred(BITS.IBackgroundCopyJob pJob)
        {
            // 0. SetNotifyCmdLine
            // 1. complete
            // 2. HandleDownloadEnded
            // 3. wait for run event
            // 4. Run
#if DEBUG
            Logger.GetLogger().Info($"JobTransferred event on download file: {_downloadFileName}");
#endif

            if (installationPackage.InstallationState > InstallationPackage.State.DownloadStart || installationPackage.InstallationState < InstallationPackage.State.Init)
                return;

            _aTimer.Stop();

            pJob.Complete();            

            try
            {
                DateTime now = DateTime.Now;
#if DEBUG
                Logger.GetLogger().Info($"setting {_downloadFileName} creation/write/access time to {now}");
#endif
                File.SetCreationTime(_downloadFileName, now);
                File.SetLastWriteTime(_downloadFileName, now);
                File.SetLastAccessTime(_downloadFileName, now);
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Warning($"unable to set {_downloadFileName} creation/write/access time: {e}");
#endif
            }

            TimerCalled();

            pJob.GetTimes(out BITS._BG_JOB_TIMES times);
            DateTime creation = MakeDateTime(times.CreationTime);
            DateTime completion = MakeDateTime(times.TransferCompletionTime);
            if ((creation > DateTime.MinValue) && (completion > DateTime.MinValue))
            {
                UpdateDownloadTime((long)(completion - creation).TotalMilliseconds);
            }

            installationPackage.HandleDownloadEnded();

            //wait on event from runner    
            if (installationPackage.RunWithBits && installationPackage.onRunWithBits.WaitOne() && installationPackage.runner.BitsEnabled)
            {
#if DEBUG
                Logger.GetLogger().Info($"running file via BITS: {installationPackage.RunFileName}");
#endif
                // Throwing with E_FAIL error-code so BITS will also execute the command line  
                throw new System.Runtime.InteropServices.COMException("", unchecked((int)0x80004005));
            }
        }

        private static DateTime MakeDateTime(BITS._FILETIME value)
        {
            long ticks = ((long)value.dwHighDateTime << 32) + (long)value.dwLowDateTime;
            return ticks == 0 ? DateTime.MinValue : DateTime.FromFileTime(ticks);
        }

        public void JobError(BITS.IBackgroundCopyJob pJob, BITS.IBackgroundCopyError pError)
        {
#if DEBUG
            Logger.GetLogger().Info($"JobError event on download file: {_downloadFileName}");
#endif
            pJob.Cancel();
            _aTimer.Stop();
            pError.GetErrorDescription((uint)CultureInfo.GetCultureInfo("en-US").LCID, out string errdesc);
            if (!string.IsNullOrWhiteSpace(errdesc))
            {
                installationPackage.HandleDownloadError(errdesc);
            }
            installationPackage.HandleProgress(installationPackage);
        }

        public void JobModification(BITS.IBackgroundCopyJob pJob, uint dwReserved)
        {
            // JobModification has to exist to satisfy the interface. But unless
            // the call to job.SetNotifyInterface includes the BG_NOTIFY_JOB_MODIFICATION flag,
            // this method won't be called.

            //This method can also be used to get progress ...

        }

        public override void Terminate()
        {
            if (_job != null)
            {
                _job.GetState(out BITS.BG_JOB_STATE state);

                switch (state)
                {
                    case BITS.BG_JOB_STATE.BG_JOB_STATE_ERROR:
                    case BITS.BG_JOB_STATE.BG_JOB_STATE_QUEUED:
                    case BITS.BG_JOB_STATE.BG_JOB_STATE_TRANSFERRING:
                    case BITS.BG_JOB_STATE.BG_JOB_STATE_SUSPENDED:
                    case BITS.BG_JOB_STATE.BG_JOB_STATE_CONNECTING:
                        CancelJob();
                        break;
                    default:
                        break;
                }
            }

            // release run event
            if (!installationPackage.runner.BitsEnabled)
                installationPackage.onRunWithBits.Set();
        }

        private void CancelJob()
        {
            try
            {
                _job?.Cancel();
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Error($"error while trying to cancel BITS job: {e.Message}");
#endif
            }
        }
    }
}
