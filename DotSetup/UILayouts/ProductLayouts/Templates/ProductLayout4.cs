// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DotSetup
{
    public partial class ProductLayout4 : ProductControl
    {
        [DllImport("user32.dll", EntryPoint = "ShowCaret")]
        public static extern long ShowCaret(IntPtr hwnd);
        [DllImport("user32.dll", EntryPoint = "HideCaret")]
        public static extern long HideCaret(IntPtr hwnd);
        private ControlsLayout _controlsLayout;
        public ProductLayout4(ControlsLayout controlsLayout)
        {
            InitializeComponent();

            Color FontColor = ConfigParser.GetConfig().GetColorValue("//Main/FormDesign/FontColor");
            if (FontColor != null)
            {
                txtDisclaimer.ForeColor = FontColor;
                txtTitle.ForeColor = FontColor;
                txtDescription.ForeColor = FontColor;
            }

            _controlsLayout = controlsLayout;
            controlsLayout.SetLayout(pnlLayout.Controls);

            Dock = DockStyle.Fill;
        }

        private void SetFontSize()
        {
            var percent = (float)Parent.Width / Width;

            txtTitle.Font = new Font(txtTitle.Font.FontFamily, (float)(txtTitle.Font.SizeInPoints * percent));
            txtTitle.Width = (int)(txtTitle.Width * percent);
            txtTitle.LineSpacing = (int)(txtTitle.LineSpacing * percent);
            txtDescription.Font = new Font(txtDescription.Font.FontFamily, (float)(txtDescription.Font.SizeInPoints * percent));
            txtDescription.LineSpacing = (int)(txtDescription.LineSpacing * percent);
        }

        public override void HandleChanges()
        {
            //SetFontSize();
            ProductLayoutUtils.MoveOptionalBadge(Parent, imgOptional);

            _controlsLayout.SetLayout(pnlLayout.Controls);
        }

        public override void HandleChanges(object sender, EventArgs e)
        {
            HandleChanges();
        }

        private void ProductLayout4_Load(object sender, EventArgs e)
        {
            txtDisclaimer.Focus(); //prevents a need to click twice on a link
            HandleChanges();
        }
    }
}
