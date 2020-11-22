// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Timers;
using BITS = BITSReference1_5;
using BITS5 = BITSReference5_0;

public enum BitsNotifyFlags : UInt32
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
        private const int MAX_TIMEOUT_SECONDS = 20;
        public PackageDownloaderBits(InstallationPackage installationPackage) : base(installationPackage)
        {
            aTimer = new Timer
            {
                Interval = 500,
                Enabled = false
            };
            aTimer.Elapsed += new ElapsedEventHandler(TimerElapsed);
        }

        public override bool Download(string downloadLink, string outFilePath)
        {
            try
            {
                mgr = new BITS.BackgroundCopyManager1_5();
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                ReportDownloadError("BackgroundCopyManager is not initialized - " + e.Message);
                return false;
            }

            if (mgr == null)
            {
                ReportDownloadError("BackgroundCopyManager is not initialized");
                return false;
            }

            try
            {
                //Can be about 60 jobs with the same for a user...
                mgr.CreateJob("DotSetup Installer", BITS.BG_JOB_TYPE.BG_JOB_TYPE_DOWNLOAD, out jobGuid, out job);
                SetJobProperties(job);

                job.AddFile(downloadLink, outFilePath);

                //Activating events for job.
                job.SetNotifyFlags(
                  (UInt32)BitsNotifyFlags.JOB_TRANSFERRED
                  + (UInt32)BitsNotifyFlags.JOB_ERROR);
                job.SetNotifyInterface(this);
                job.Resume();  //starting the job
            }
            catch (System.Exception e)
            {
                ReportDownloadError(e.Message);
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
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            TimerCalled();
        }

        private void TimerCalled()
        {
            job.GetState(out BITS.BG_JOB_STATE state);
            if (state == BITS.BG_JOB_STATE.BG_JOB_STATE_CONNECTING ||
                state == BITS.BG_JOB_STATE.BG_JOB_STATE_TRANSIENT_ERROR)
            {
                //job in a state that that is connecting or in no connection error
                if (timeCounter >= (MAX_TIMEOUT_SECONDS / 2)) //20 seconds of timeout 
                {
                    CancelJob();
                    aTimer.Stop();
                    ReportDownloadError("No Internet connection");
                }
                else
                    timeCounter++;
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

        public void JobTransferred(BITS.IBackgroundCopyJob pJob)
        {
            aTimer.Stop();
            pJob.Complete();
            TimerCalled();
            installationPackage.HandleDownloadEnded();
        }

        public void JobError(BITS.IBackgroundCopyJob pJob, BITS.IBackgroundCopyError pError)
        {
            pJob.Cancel();
            aTimer.Stop();
            pError.GetErrorDescription((uint)System.Threading.Thread.CurrentThread.CurrentCulture.LCID, out string errdesc);
            if (errdesc != null)
            {
                ReportDownloadError(errdesc);
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
