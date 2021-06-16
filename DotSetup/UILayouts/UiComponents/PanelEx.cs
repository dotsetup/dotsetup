// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.Drawing;
using System.Windows.Forms;
using DotSetup.UILayouts.ProductLayouts;

namespace DotSetup.UILayouts.UIComponents
{
    public class PanelEx : Panel
    {
        public PanelEx() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams handleParam = base.CreateParams;
                if (ProductLayoutUtils.isSDK)
                    return handleParam;
                handleParam.ExStyle |= 0x02000000;   // WS_EX_COMPOSITED       
                return handleParam;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        // Paint background with underlying graphics from other controls
        {
            base.OnPaintBackground(e);

            if (BackColor == Color.Transparent)
            {
                Graphics g = e.Graphics;

                if (Parent != null)
                {
                    // Take each control in turn
                    int index = Parent.Controls.GetChildIndex(this);
                    for (int i = Parent.Controls.Count - 1; i > index; i--)
                    {
                        Control c = Parent.Controls[i];

                        // Check it's visible and overlaps this control
                        if (c.Bounds.IntersectsWith(Bounds) && c.Visible)
                        {
                            // Load appearance of underlying control and redraw it on this background
                            Bitmap bmp = new Bitmap(c.Width, c.Height, g);
                            c.DrawToBitmap(bmp, c.ClientRectangle);
                            g.TranslateTransform(c.Left - Left, c.Top - Top);
                            g.DrawImageUnscaled(bmp, Point.Empty);
                            g.TranslateTransform(Left - c.Left, Top - c.Top);
                            bmp.Dispose();
                        }
                    }
                }
            }
        }


        public void SetImage(string imageName, string decode) => BackgroundImage = UICommon.PrepareImage(imageName, decode, (img) => { BackgroundImage = img; });
    }
}
