// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DotSetup
{
    internal class OptLayout
    {
        public static class OptType { public const string IN = "IN", OUT = "OUT", SMART = "SMART"; }

        public CheckBox optIn;
        public RadioButton smOptInY;
        public RadioButton smOptInN;
        public RichTextBoxEx smOptInText;
        public PictureBoxEx smOptInBg;
        public RichTextBoxEx txtTitle;
        public Panel darkenProductOverlay;
        public bool smOptShown;
        public string opt;

        private readonly Panel pnlLayout;
        private Bitmap inst;
        private Bitmap offe;
        private readonly PictureBox imgDarkenOverlay;
        private Timer timer;
        private int blinkCounter;
        private readonly int blinkMax;
        private bool exitFlag;

        public OptLayout(Panel layout, int blinkMax)
        {
            pnlLayout = layout;
            optIn = pnlLayout.Controls.Find("txtOptIn", true).FirstOrDefault() as CheckBox;
            smOptInY = pnlLayout.Controls.Find("txtSmOptInY", true).FirstOrDefault() as RadioButton;
            smOptInN = pnlLayout.Controls.Find("txtSmOptInN", true).FirstOrDefault() as RadioButton;
            smOptInText = pnlLayout.Controls.Find("txtSmOptIn", true).FirstOrDefault() as RichTextBoxEx;
            smOptInBg = pnlLayout.Controls.Find("imgSmOpInBg", true).FirstOrDefault() as PictureBoxEx;
            txtTitle = pnlLayout.Controls.Find("txtTitle", true).FirstOrDefault() as RichTextBoxEx;
            darkenProductOverlay = pnlLayout.Controls.Find("pnlDarkenOverlay", true).FirstOrDefault() as Panel;
            darkenProductOverlay.Size = pnlLayout.Size;

            imgDarkenOverlay = new PictureBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location = pnlLayout.Location,
                Name = "imgDarkenOverlay",
                Size = pnlLayout.Size,
                TabStop = false,
                Visible = false
            };
            pnlLayout.Controls.Add(imgDarkenOverlay);

            string opt = ConfigParser.GetConfig().GetStringValue("//RemoteConfiguration/FlowSettings/OptType", OptType.IN);
            if (opt == OptType.SMART && (smOptInText == null || String.IsNullOrEmpty(smOptInText.Text))) // Fallback if the remote config not relevant
                opt = OptType.IN;
            if (opt == OptType.IN)
            {
                optIn.Visible = true;
            }
            else if (opt == OptType.SMART)
            {
                smOptInY.Visible = true;
                smOptInN.Visible = true;
            }
            this.opt = opt;
            this.blinkMax = blinkMax;
            InitTimer();
        }

        public void ShowSmOptInOverlay()
        {
            DarkenInstaller();
            smOptInText.Visible = true;
            smOptInBg.Visible = true;
            smOptShown = true;
            darkenProductOverlay.Controls.Add(smOptInBg);
            darkenProductOverlay.Controls.Add(smOptInY);
            darkenProductOverlay.Controls.Add(smOptInN);
            darkenProductOverlay.Controls.Add(smOptInText);
            smOptInText.BringToFront();
            smOptInY.BringToFront();
            smOptInN.BringToFront();
            smOptInY.ForeColor = Color.FromArgb(0, 0, 0);
            smOptInY.BackColor = Color.FromArgb(232, 232, 232);
            smOptInN.ForeColor = Color.FromArgb(0, 0, 0);
            smOptInN.BackColor = Color.FromArgb(232, 232, 232);
            darkenProductOverlay.BringToFront();
        }

        public void DarkenInstaller()
        {
            offe = DarkenControl(darkenProductOverlay, pnlLayout);
            inst = DarkenControl(imgDarkenOverlay, imgDarkenOverlay);
            darkenProductOverlay.BackgroundImage = offe;
            imgDarkenOverlay.BackgroundImage = inst;
            darkenProductOverlay.Visible = true;
            imgDarkenOverlay.Visible = true;
        }

        public Bitmap DarkenControl(Control controlToDarken, Control controlRefLocation)
        {
            // take a screenshot of the form and darken it:
            Bitmap bmp = new Bitmap(controlToDarken.Width, controlToDarken.Height);
            using (Graphics G = Graphics.FromImage(bmp))
            {
                G.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                G.CopyFromScreen(controlRefLocation.PointToScreen(new Point(0, 0)), new Point(0, 0), controlToDarken.Size);
                double percent = 0.60;
                Color darken = Color.FromArgb((int)(255 * percent), Color.Black);
                using (Brush brsh = new SolidBrush(darken))
                {
                    G.FillRectangle(brsh, controlToDarken.DisplayRectangle);
                }
            }
            return bmp;
        }

        public void RemoveDarken()
        {
            darkenProductOverlay.Visible = false;
            imgDarkenOverlay.Visible = false;
        }

        public void InitTimer()
        {
            timer = new Timer();
            timer.Tick += new EventHandler(TimerEventProcessor);
            timer.Interval = 200;
        }

        // This is the method that will run on every timer tick
        public void TimerEventProcessor(object myObject, EventArgs myEventArgs)
        {
            timer.Stop();

            if (blinkCounter < blinkMax)
            {
                smOptInText.Visible = !smOptInText.Visible;
                smOptInY.Visible = !smOptInY.Visible;
                smOptInN.Visible = !smOptInN.Visible;
                smOptInBg.Visible = !smOptInBg.Visible;

                // Restarts the timer and increments the counter.
                blinkCounter += 1;
                timer.Enabled = true;
            }
            else
            {
                // Stops the timer.
                exitFlag = true;
                blinkCounter = 0;
            }
        }

        public void BlinkSmartOptin()
        {
            exitFlag = false;

            timer.Start();

            while (exitFlag == false)
            {
                // Processes all the events in the queue.
                Application.DoEvents();
            }
        }
    }
}
