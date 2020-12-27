// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DotSetup
{
    public partial class ProductLayout1 : ProductControl
    {
        [DllImport("user32.dll", EntryPoint = "ShowCaret")]
        public static extern long ShowCaret(IntPtr hwnd);
        [DllImport("user32.dll", EntryPoint = "HideCaret")]
        public static extern long HideCaret(IntPtr hwnd);
        public ProductLayout1(ControlsLayout controlsLayout)
        {
            InitializeComponent();

            controlsLayout.SetLayout(pnlLayout.Controls);

            Dock = DockStyle.Fill;
        }

        private void ProductLayout1_Load(object sender, EventArgs e)
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
            }
        }
    }
}
