// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DotSetup.UILayouts.UIComponents
{
    public class RoundButton : Button
    {
        private readonly int padding = 5;
        private readonly int cornerRadius = 38;
        private readonly Color borderColor = Color.Black;
        private readonly int borderSize = 1;

        public RoundButton() : base()
        {
            FlatStyle = FlatStyle.Flat;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            // to draw the control using base OnPaint
            base.OnPaint(e);

            //to modify the corner radius
            Pen DrawPen = new Pen(borderColor, borderSize);
            GraphicsPath gfxPath_mod = new GraphicsPath();

            int top = padding;
            int left = padding;
            int right = Width - padding;
            int bottom = Height - padding;

            gfxPath_mod.AddArc(left, top, cornerRadius, cornerRadius, 180, 90);
            gfxPath_mod.AddArc(right - cornerRadius, top, cornerRadius, cornerRadius, 270, 90);
            gfxPath_mod.AddArc(right - cornerRadius, bottom - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            gfxPath_mod.AddArc(left, bottom - cornerRadius, cornerRadius, cornerRadius, 90, 90);

            gfxPath_mod.CloseAllFigures();

            e.Graphics.DrawPath(DrawPen, gfxPath_mod);

            Region = new Region(gfxPath_mod);

        }
    }
}
