// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.Drawing;
using System.Windows.Forms;

namespace DotSetup.UILayouts.UIComponents
{
    public class Divider : Control
    {
        public Divider()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
            MaximumSize = new Size(600, 1);
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0, 0, Width, Height);
            Brush brush = new SolidBrush(BackColor);
            e.Graphics.FillRectangle(brush, 0, 0, rect.Width, rect.Height);
        }
    }
}
