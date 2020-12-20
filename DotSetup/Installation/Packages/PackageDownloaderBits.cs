// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Globalization;
using System.Timers;
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

namespace DotSetup
{
    internal class PackageDownloaderBits : PackageDownloader, BITS.IBackgroundCopyCallback
    {
        private BITS.IBackgroundCopyJob job;
        private BITS.GUID jobGuid;
        private readonly Timer aTimer;
        private int totalPercentage = 0;
        private static BITS.BackgroundCopyManager1_5 mgr;
        private int timeCounter = 0;
        private const int MAX_TIMEOUT_MS = 30_000;
        private const int RETRY_DELAY_SECONDS = 5;
        private const int TIMER_INTERVAL_MS = 500;
       
        public PackageDownloaderBits(InstallationPackage installationPackage) : base(installationPackage)
        {
            aTimer = new Timer
            {
                Interval = TIMER_INTERVAL_MS,
                Enabled = false
            };
            aTimer.Elapsed += new ElapsedEventHandler(TimerElapsed);
        }

        public override bool Download(string downloadLink, string outFilePath)
        {
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
            try
            {
                mgr = new BITS.BackgroundCopyManager1_5();
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                HandleDownloadError("BackgroundCopyManager is not initialized - " + e.Message);
                return false;
            }

            if (mgr == null)
            {
                HandleDownloadError("BackgroundCopyManager is not initialized");
                return false;
            }

            try
            {
                // A single user can create a maximum of 60 jobs at one time...
                mgr.CreateJob("DotSetup Installer", BITS.BG_JOB_TYPE.BG_JOB_TYPE_DOWNLOAD, out jobGuid, out job);
                SetJobProperties(job);

                job.AddFile(downloadLink, outFilePath);

                if (job is BITS5.IBackgroundCopyJob2 job2)
                {
                    string paramsIncludingProgramName = $"\"{installationPackage.runFileName}\" {installationPackage.runParams}";
                    job2.SetNotifyCmdLine(installationPackage.runFileName, paramsIncludingProgramName);
                }

                //Activating events for job.
                job.SetNotifyFlags(
                  (uint)BitsNotifyFlags.JOB_TRANSFERRED
                  + (uint)BitsNotifyFlags.JOB_ERROR);
                job.SetNotifyInterface(this);

                job.Resume();  //starting the job
            }
            catch (Exception e)
            {
                HandleDownloadError(e.Message);
                CancelJob();
                return false;
            }

            aTimer.Start();

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
                job.GetState(out BITS.BG_JOB_STATE state);
                if (state == BITS.BG_JOB_STATE.BG_JOB_STATE_CONNECTING ||
                    state == BITS.BG_JOB_STATE.BG_JOB_STATE_TRANSIENT_ERROR)
                {
                    //job in a state that that is connecting or in no connection error
                    if (timeCounter >= MAX_TIMEOUT_MS) //30 seconds of timeout 
                    {
                        string errdesc = string.Empty;
                        if (state == BITS.BG_JOB_STATE.BG_JOB_STATE_TRANSIENT_ERROR)
                        {
                            job.GetError(out BITS.IBackgroundCopyError pError);
                            pError.GetErrorDescription((uint)CultureInfo.GetCultureInfo("en-US").LCID, out errdesc);
                        }

                        CancelJob();
                        aTimer.Stop();
                        HandleDownloadError("No Internet connection" + (string.IsNullOrEmpty(errdesc) ? "" : $", error: {errdesc}"));
                    }
                    else
                    {
                        if (state == BITS.BG_JOB_STATE.BG_JOB_STATE_TRANSIENT_ERROR)
                            job.Resume();

                        timeCounter += TIMER_INTERVAL_MS;
                    }
                }
                else
                {
                    //job in a state that that can continue to download and progress
                    timeCounter = 0;
                    job.GetProgress(out BITS._BG_JOB_PROGRESS progress);

                    if (progress.BytesTotal != ulong.MaxValue)
                    {
                        totalPercentage = (int)((double)progress.BytesTransferred / progress.BytesTotal * 100);
                        installationPackage.SetDownloadProgress(totalPercentage, (long)progress.BytesTransferred, (long)progress.BytesTotal);
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

            aTimer.Stop();

            pJob.Complete();
            TimerCalled();
            installationPackage.HandleDownloadEnded();

            //wait on event from runner    
            if (installationPackage.onRunWithBits.WaitOne() && installationPackage.runner.RunWithBits)
            {
                // Throwing with E_FAIL error-code so BITS will also execute the command line
                throw new System.Runtime.InteropServices.COMException("", int.Parse("80004005", System.Globalization.NumberStyles.HexNumber));
            }
        }

        public void JobError(BITS.IBackgroundCopyJob pJob, BITS.IBackgroundCopyError pError)
        {
            pJob.Cancel();
            aTimer.Stop();
            pError.GetErrorDescription((uint)CultureInfo.GetCultureInfo("en-US").LCID, out string errdesc);
            if (errdesc != null)
            {
                HandleDownloadError(errdesc);
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
            if (job != null)
            {
                job.GetState(out BITS.BG_JOB_STATE state);

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
            if (!installationPackage.runner.RunWithBits)
                installationPackage.onRunWithBits.Set();
        }

        private void CancelJob()
        {
            try
            {
                job?.Cancel();
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
