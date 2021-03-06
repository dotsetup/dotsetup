﻿// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DotSetup.Infrastructure;
using DotSetup.UILayouts.UIComponents;

namespace DotSetup.Installation.WinForm
{
    public partial class FrmParent : Form
    {
        public PanelEx pnlContent;

        public FrmParent()
        {
            InitializeComponent();

            SetTaskbarIcon();
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;
        [DllImport("User32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public void Draggable_OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        private const int WS_MINIMIZEBOX = 0x20000;
        private const int CS_DBLCLKS = 0x8;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= WS_MINIMIZEBOX;
                cp.ClassStyle |= CS_DBLCLKS;
                return cp;
            }
        }

        public void SetTaskbarIcon()
        {
            if (ResourcesUtils.EmbeddedResourceExists(null, ".ico"))
                Icon = new System.Drawing.Icon(ResourcesUtils.GetEmbeddedResourceStream(null, ".ico"));
            else
            {
#if DEBUG
                Logger.GetLogger().Warning("Set at least one icon as embedded resource so that it will be found by " + GetType().Name);
#endif
            }
        }
    }
}
