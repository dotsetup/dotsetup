// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DotSetup.Infrastructure;

namespace DotSetup.UILayouts.UIComponents
{
    public class ProgressBarEx : ProgressBar
    {
        private Color _ColorLeft = Color.FromArgb(255, 0, 182, 255);
        private Color _ColorRight = Color.FromArgb(255, 0, 88, 206);
        private readonly Timer timer = new Timer();
        private int x;
        private int _Radius = 10;
        private int _BorderThickness = 0;
        public int ChunksWidth { get; set; }

        public ProgressBarEx()
        {
            SetStyle(ControlStyles.Opaque, false);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);

            timer.Interval = 10;
            timer.Tick += (s, e) => { x = (x + 5) % Width; Invalidate(); };

            Disposed += (s, e) => timer.Dispose();
            ChunksWidth = 100;
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

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var Rect = new RectangleF(0, 0, Width, Height);
            GraphicsPath GraphPath = GraphicsUtils.GetRoundPath(Rect, _Radius);

            Region = new Region(GraphPath);
            using var pen = new Pen(Color.Silver, _BorderThickness);
            pen.Alignment = PenAlignment.Inset;
            e.Graphics.DrawPath(pen, GraphPath);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using Image offscreenImage = new Bitmap(Width, Height);
            using var offscreen = Graphics.FromImage(offscreenImage);

            const int inset = 2;
            const int rand = 2;

            var rect = new Rectangle(0, 0, Width, Height);
            rect.Inflate(new Size(-inset, -inset));

            var brush = new LinearGradientBrush(rect, ColorLeft, ColorRight, LinearGradientMode.Horizontal);

            if (ProgressBarRenderer.IsSupported)
                ProgressBarRenderer.DrawHorizontalBar(offscreen, rect);


            e.Graphics.DrawImage(offscreenImage, 0, 0, new Rectangle(new Point(0, 0), new Size(Width, Height)), GraphicsUnit.Pixel);
            offscreen.FillRectangle(brush, inset, inset, rect.Width, rect.Height);

            if (Style == ProgressBarStyle.Marquee)
            {
                if (!timer.Enabled)
                    timer.Start();
                var section = new Rectangle(new Point(x + rand, rand), new Size(ChunksWidth - 2 * rand, Height - 2 * rand));
                e.Graphics.DrawImage(offscreenImage, x + rand, rand, section, GraphicsUnit.Pixel);
                if (x + ChunksWidth > Width)
                {
                    section = new Rectangle(new Point(rand, rand), new Size(x + ChunksWidth - Width - 2 * rand, Height - 2 * rand));
                    e.Graphics.DrawImage(offscreenImage, rand, rand, section, GraphicsUnit.Pixel);
                }
            }
            else
            {
                if (timer.Enabled)
                    timer.Stop();
                var Width = (int)(this.Width * ((Value - (double)Minimum) / (Maximum - (double)Minimum)));
                if (Width == 0) Width = 1;

                var section = new Rectangle(new Point(0, 0), new Size(Width, Height));
                e.Graphics.DrawImage(offscreenImage, 0, 0, section, GraphicsUnit.Pixel);
            }


            if (Text != "")
            {
                var size = e.Graphics.MeasureString(Text, Font);
                var middleLeft = new PointF(10, Height / 2 - size.Height / 2);
                e.Graphics.DrawString(Text, Font, new SolidBrush(ForeColor), middleLeft);
            }
        }
    }
}
