// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Windows.Forms;

namespace DotSetup
{
    public partial class FormTestSDK : Form
    {
        public FormTestSDK()
        {
            InitializeComponent();
        }

        [STAThread]
        //TODO: add "string[] args" and use it
        public static void Main()
        {
            DotSetupSDK.InitApplication();
            Form frm = new FormTestSDK();

            Invoke(Exports.ExportedFunctions.Init, new { AccountID = "TestAccount" });
            Application.Run(frm);
        }

        private void ButtonSetWindow_Click(object sender, EventArgs e)
        {
            Invoke(Exports.ExportedFunctions.SetWindow, new { Wnd = this.Handle, X = 25, Y = 25, Width = 660, Height = 400 });
            if (DotSetupSDK.pnlLayout != null)
            {
                DotSetupSDK.pnlLayout.BorderStyle = BorderStyle.FixedSingle;
                ButtonShowHide_Click(sender, e);
            }
        }

        private void ButtonAccept_Click(object sender, EventArgs e)
        {
            int hasNext = Invoke(Exports.ExportedFunctions.Accept);
            if ((hasNext == 0) && (ButtonShowHide.Text == "Hide"))
                ButtonShowHide_Click(sender, e);
        }

        private void ButtonDecline_Click(object sender, EventArgs e)
        {
            int hasNext = Invoke(Exports.ExportedFunctions.Decline);
            if ((hasNext == 0) && (ButtonShowHide.Text == "Hide"))
                ButtonShowHide_Click(sender, e);
        }

        private void ButtonShowHide_Click(object sender, EventArgs e)
        {
            if (ButtonShowHide.Text == "Show")
            {
                Invoke(Exports.ExportedFunctions.Show);
                ButtonShowHide.Text = "Hide";
            }
            else
            {
                Invoke(Exports.ExportedFunctions.Hide);
                ButtonShowHide.Text = "Show";
            }
        }

        private void ButtonInstall_Click(object sender, EventArgs e)
        {
            Invoke(Exports.ExportedFunctions.Install);
            TimerProgress.Enabled = true;
            TimerProgress.Start();
        }

        private void FormTestSDK_FormClosing(object sender, FormClosingEventArgs e)
        {
            Invoke(Exports.ExportedFunctions.Finalize);
        }

        private void TimerProgress_Tick(object sender, EventArgs e)
        {
            int progress = Invoke(Exports.ExportedFunctions.GetProgress);
            ProgressBar1.Value = progress;
            if (progress == 100)
            {
                TimerProgress.Stop();
                TimerProgress.Enabled = false;
            }
        }

        private static int Invoke(Exports.ExportedFunctions func, object obj = null)
        {
            return Exports.Invoke((int)func, JSONParser.ObjToJSON(obj));
        }
    }
}
