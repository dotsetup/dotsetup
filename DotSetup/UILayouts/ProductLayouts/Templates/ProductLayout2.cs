// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DotSetup.Installation.Configuration;
using DotSetup.UILayouts.ProductLayouts;

namespace DotSetup.UILayouts.ControlLayout.Templates
{
    public partial class ProductLayout2 : ProductControl
    {
        [DllImport("user32.dll", EntryPoint = "ShowCaret")]
        public static extern long ShowCaret(IntPtr hwnd);
        [DllImport("user32.dll", EntryPoint = "HideCaret")]
        public static extern long HideCaret(IntPtr hwnd);

        public ProductLayout2(ControlsLayout controlsLayout)
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

        public override void HandleChanges()
        {
            ProductLayoutUtils.ResizeBackground(Parent, imgBackground, txtDisclaimer);
            ProductLayoutUtils.MoveOptionalBadge(Parent, imgOptional);
            ProductLayoutUtils.MoveDisclaimer(Parent, txtDisclaimer);
        }

        public override void HandleChanges(object sender, EventArgs e)
        {
            HandleChanges();
        }

        private void ProductLayout2_Load(object sender, EventArgs e)
        {
            txtDisclaimer.Focus();
        }
    }
}
