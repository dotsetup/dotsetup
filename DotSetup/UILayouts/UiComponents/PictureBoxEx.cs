// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DotSetup.UILayouts.UIComponents
{
    public class PictureBoxEx : PictureBox
    {
		private bool _rounded = false;
        public PictureBoxEx() : base()
        {
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
        }

        public bool Rounded
        {
            get { return _rounded; }
            set
            {
                _rounded = value;
                Invalidate();
            }
        }

        public void SetImage(string imageName, string decode)
        {
            Image = UICommon.PrepareImage(imageName, decode);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            if (BackColor == Color.Transparent)
            {
                if (Parent != null)
                {
                    int index = Parent.Controls.GetChildIndex(this);
                    for (int i = Parent.Controls.Count - 1; i > index; i--)
                    {
                        Control c = Parent.Controls[i];

                        if (c.Bounds.IntersectsWith(Bounds) && c.Visible)
                        {
                            Bitmap bmp = new Bitmap(c.Width, c.Height, e.Graphics);
                            c.DrawToBitmap(bmp, c.ClientRectangle);
                            e.Graphics.TranslateTransform(c.Left - Left, c.Top - Top);
                            e.Graphics.DrawImageUnscaled(bmp, Point.Empty);
                            e.Graphics.TranslateTransform(Left - c.Left, Top - c.Top);
                            bmp.Dispose();
                        }
                    }
                }
            }
            else if (Rounded == true)
            {
                using GraphicsPath gp = new GraphicsPath();
                gp.AddEllipse(0, 0, Width, Height);
                Region = new Region(gp);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawEllipse(new Pen(new SolidBrush(BackColor), 1), 0, 0, Width, Height);
            }
        }
    }
}
