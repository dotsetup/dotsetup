// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DotSetup.Infrastructure;

namespace DotSetup.UILayouts.UIComponents
{
    public class ButtonEx : Button
    {
        private Color _ColorLeft = Color.FromArgb(255, 0, 182, 255);
        private Color _ColorRight = Color.FromArgb(255, 0, 88, 206);
        private int _Radius = 10;
        private int _BorderThickness = 0;
        private bool _TextUpperCase = false;

        public ButtonEx ()
        {
            SetStyle(ControlStyles.Opaque, false);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
        }

        public Color ColorLeft
        {
            get { return _ColorLeft; }
            set
            {
                _ColorLeft = value;
                Invalidate();
            }
        }

        public Color ColorRight
        {
            get { return _ColorRight; }
            set
            {
                _ColorRight = value;
                Invalidate();
            }
        }

        public int Radius
        {
            get { return _Radius; }
            set
            {
                _Radius = value;
                Invalidate();
            }
        }

        public int BorderThickness
        {
            get { return _BorderThickness; }
            set
            {
                _BorderThickness = value;
                Invalidate();
            }
        }

        public bool TextUpperCase
        {
            get { return _TextUpperCase; }
            set
            {
                _TextUpperCase = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            RectangleF Rect = new RectangleF(0, 0, Width, Height);
            GraphicsPath GraphPath = GraphicsUtils.GetRoundPath(Rect, _Radius);

            Region = new Region(GraphPath);
            using (Pen pen = new Pen(BackColor, _BorderThickness))
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawPath(pen, GraphPath);
            }

            e.Graphics.FillRectangle(new LinearGradientBrush(new PointF(0, Height / 2), new PointF(Width, Height / 2), ColorLeft, ColorRight), ClientRectangle);

            string Text = (_TextUpperCase) ? this.Text.ToUpper() : this.Text;

            SizeF size = e.Graphics.MeasureString(Text, Font);
            PointF middleLeft = new PointF(Width / 2 - size.Width / 2, Height / 2 - size.Height / 2);
            e.Graphics.DrawString(Text, Font, new SolidBrush(ForeColor), middleLeft);
        }
    }
}
