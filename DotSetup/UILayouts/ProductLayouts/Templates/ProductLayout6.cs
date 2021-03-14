// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DotSetup
{
    public partial class ProductLayout6 : ProductControl
    {
        [DllImport("user32.dll", EntryPoint = "ShowCaret")]
        public static extern long ShowCaret(IntPtr hwnd);
        [DllImport("user32.dll", EntryPoint = "HideCaret")]
        public static extern long HideCaret(IntPtr hwnd);

        public ProductLayout6(ControlsLayout controlsLayout)
        {
            InitializeComponent();

            Color FontColor = ConfigParser.GetConfig().GetColorValue("//Main/FormDesign/FontColor");
            if (FontColor != null)
            {
                txtDisclaimer.ForeColor = FontColor;
                txtDescription.ForeColor = FontColor;
                txtTitle.ForeColor = FontColor;
            }

            controlsLayout.SetLayout(pnlLayout.Controls);

            Dock = DockStyle.Fill;
        }

        public override void HandleChanges()
        {
            ProductLayoutUtils.ResizeBackground(Parent, imgBackground, txtDisclaimer);
            ProductLayoutUtils.MoveOptionalBadge(Parent, imgOptional);
            ProductLayoutUtils.MoveDisclaimer(Parent, txtDisclaimer);

            txtDescription.Height = txtDisclaimer.Location.Y;
            txtDescription.Width = Parent.Width / 2;
        }

        public override void HandleChanges(object sender, EventArgs e)
        {
            HandleChanges();
        }

        private void ProductLayout6_Load(object sender, EventArgs e)
        {
            txtDisclaimer.Focus();

            if (imgTitle.Image == null)
            {
                pnlLayout.Controls.Remove(imgTitle);
                imgTitle.Dispose();
            }
            else
            {
                imgTitle.BackColor = Color.Transparent;
                imgTitle.Parent = imgBackground;
            }
        }
    }
}
