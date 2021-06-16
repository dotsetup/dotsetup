// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Windows.Forms;
using DotSetup.Infrastructure;
using DotSetup.Installation.Configuration;
using DotSetup.Installation.Events;

namespace DotSetup.Installation.WinForm
{

    public class ProgressBarUpdater
    {
        public static string lastErrorMessage, statText, statInst, statFinished, statMain, statCompleted;
        public static ProgressBar pbProgressBar;
        public static Control lblProgressValue;
        public static Control lblProgressUpperText;
        public static Button btnRetry;
        public static Button btnFinish;
        public static event Action<ProgressEventArgs> OnProgressUpdate;
        public static int currentState, currentProgress = 0, TimerProgress = 0;
        public static Random rand = new Random();
        public static Timer fakeProgressTimer = new Timer();
        public static bool isActive;

        public static void SetComponents(ProgressBar _pbProgressBar, Control _lblProgressValue, Control _lblProgressUpperText, Button _btnRetry, Button _btnFinish)
        {
            isActive = true;
            statText = ConfigParser.GetLocalizedMessage("PROGRESS_STAT_TEXT", "out of");
            statInst = ConfigParser.GetLocalizedMessage("PROGRESS_STAT_INSTALLING", "Installing...");
            statFinished = ConfigParser.GetLocalizedMessage("PROGRESS_STAT_FINISHED");
            statMain = ConfigParser.GetLocalizedMessage("PROGRESS_STATUS_MAIN");
            statCompleted = ConfigParser.GetLocalizedMessage("PROGRESS_STATUS");
            pbProgressBar = _pbProgressBar;
            lblProgressValue = _lblProgressValue;
            lblProgressUpperText = _lblProgressUpperText;
            btnRetry = _btnRetry;
            btnFinish = _btnFinish;
            SetComponentStartState();
            StartFakeProgressTimer();
        }

        private static void StartFakeProgressTimer()
        {
            fakeProgressTimer.Tick += new EventHandler(TimerEventProcessor);
            fakeProgressTimer.Interval = rand.Next(1000, 3000);
            fakeProgressTimer.Start();
        }

        public static void TimerEventProcessor(object myObject, EventArgs myEventArgs)
        {
            fakeProgressTimer.Interval = rand.Next(3000, 5000);

            if (pbProgressBar != null && currentProgress == TimerProgress && TimerProgress < 10)
            {
                TimerProgress = currentProgress + 1;
                ProgressEventArgs progressEvent = new ProgressEventArgs("", TimerProgress, 0, 0, 0, false);
                HandleProgress(progressEvent);
            }
            else
            {
                TimerProgress = currentProgress;
            }
        }

        public static void RetryClicked()
        {
            lastErrorMessage = "";
            btnRetry.Hide();
            SetComponentStartState();
        }

        public static void SetComponentStartState()
        {
            pbProgressBar.Value = 0;
            btnFinish.Hide();
            btnRetry.Hide();
        }

        public static void HandleProgress(EventArgs progressEventArgs)
        {
            ProgressEventArgs progressEvent = (ProgressEventArgs)progressEventArgs;
            currentState = progressEvent.state;
            if (currentProgress < progressEvent.downloadPercentage)
                currentProgress = progressEvent.downloadPercentage;
            if (!isActive)
                return;
            OnProgressUpdate?.Invoke(progressEvent);
            pbProgressBar?.PerformSafely(() => pbProgressBar.Value = currentProgress);
            if (progressEvent.state == ProgressEventArgs.State.Run)
            {
                fakeProgressTimer.Stop();
                pbProgressBar?.PerformSafely(() => pbProgressBar.Style = ProgressBarStyle.Marquee);
                lblProgressValue?.PerformSafely(() => lblProgressValue.Text = statInst);
            }
            else if (progressEvent.state == ProgressEventArgs.State.Download)
            {
                lblProgressValue?.PerformSafely(() => lblProgressValue.Text = statCompleted.Replace("$percent$", currentProgress + "%"));
            }
            else if (progressEvent.state == ProgressEventArgs.State.Done)
            {
                fakeProgressTimer.Stop();
                pbProgressBar?.PerformSafely(() => pbProgressBar.Style = ProgressBarStyle.Continuous);
                lblProgressValue?.PerformSafely(() => lblProgressValue.Text = statCompleted.Replace("$percent$", "100%"));
                lblProgressUpperText?.PerformSafely(() => lblProgressUpperText.Text = statFinished);
                btnFinish?.PerformSafely(() => btnFinish.Show());
            }
        }

        internal static void Close()
        {
            isActive = false;
            fakeProgressTimer.Stop();
        }
    }
}
