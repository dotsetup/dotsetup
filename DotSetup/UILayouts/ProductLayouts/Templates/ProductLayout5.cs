// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DotSetup
{
    public partial class ProductLayout5 : ProductControl
    {
        [DllImport("user32.dll", EntryPoint = "ShowCaret")]
        public static extern long ShowCaret(IntPtr hwnd);
        [DllImport("user32.dll", EntryPoint = "HideCaret")]
        public static extern long HideCaret(IntPtr hwnd);
        public ProductLayout5(ControlsLayout controlsLayout)
        {
            InitializeComponent();

            Color FontColor = ConfigParser.GetConfig().GetColorValue("//Main/FormDesign/FontColor");
            if (FontColor != null)
            {
                txtDisclaimer.ForeColor = FontColor;
                txtTitle.ForeColor = FontColor;
                txtDescription.ForeColor = FontColor;
            }

            controlsLayout.SetLayout(pnlLayout.Controls);

            Dock = DockStyle.Fill;
        }

        private void ProductLayout5_Load(object sender, EventArgs e)
        {
            txtDisclaimer.Focus(); //prevents a need to click twice on a link

            if (imgTitle.Image == null)
            {
                pnlLayout.Controls.Remove(imgTitle);
                imgTitle.Dispose();
            }
            else
            {
                imgTitle.BackColor = System.Drawing.Color.Transparent;
                imgTitle.Parent = imgBackground;
            }
        }
    }
}
