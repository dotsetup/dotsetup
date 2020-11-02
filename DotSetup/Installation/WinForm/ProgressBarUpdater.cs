// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Windows.Forms;

namespace DotSetup
{

    public class ProgressBarUpdater
    {
        public static string lastErrorMessage, statText, statInst, statFinished, statMain, statCompleted;
        public static ProgressBar pbProgressBar;
        public static Control lblProgressValue;
        public static Control lblProgressUpperText;
        public static Button btnRetry;
        public static Button btnFinish;

        public static void SetComponents(ProgressBar _pbProgressBar, Control _lblProgressValue, Control _lblProgressUpperText, Button _btnRetry, Button _btnFinish)
        {
            statText = ConfigParser.GetConfig().GetStringValue("//Locale/PROGRESS_STAT_TEXT");
            statInst = ConfigParser.GetConfig().GetStringValue("//Locale/PROGRESS_STAT_INSTALLING");
            statFinished = ConfigParser.GetConfig().GetStringValue("//Locale/PROGRESS_STAT_FINISHED");
            statMain = ConfigParser.GetConfig().GetStringValue("//Locale/PROGRESS_STATUS_MAIN");
            statCompleted = ConfigParser.GetConfig().GetStringValue("//Locale/PROGRESS_STATUS");
            pbProgressBar = _pbProgressBar;
            lblProgressValue = _lblProgressValue;
            lblProgressUpperText = _lblProgressUpperText;
            btnRetry = _btnRetry;
            btnFinish = _btnFinish;
            SetComponentStartState();
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
            pbProgressBar?.PerformSafely(() => pbProgressBar.Value = progressEvent.downloadPercentage);
            lblProgressValue?.PerformSafely(() => lblProgressValue.Text = statCompleted.Replace("$percent$", progressEvent.downloadPercentage + "%"));
            if (progressEvent.state == ProgressEventArgs.State.Done)
            {
                lblProgressUpperText?.PerformSafely(() => lblProgressUpperText.Text = statFinished);
                btnFinish?.PerformSafely(() => btnFinish.Show());
            }
        }
    }
}
