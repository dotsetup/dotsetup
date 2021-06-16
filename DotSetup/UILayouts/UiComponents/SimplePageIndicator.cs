// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.Drawing;
using System.Windows.Forms;

namespace DotSetup.UILayouts.UIComponents
{
    public partial class SimplePageIndicator : UserControl
    {
        private readonly Graphics graphics;
        private Pen checkedPen, uncheckedPen;
        private Brush checkedBrush, uncheckedBrush;
        private int totalPageCount, currntPageIndex;

        public SimplePageIndicator()
        {
            InitializeComponent();
            graphics = CreateGraphics();
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            totalPageCount = 0;
        }

        public void SetCount(int pageCount, int checkedPageIndex, Color checkedColor, Color uncheckedColor)
        {
            checkedPen = new Pen(checkedColor);
            uncheckedPen = new Pen(uncheckedColor);
            checkedBrush = new SolidBrush(checkedColor);
            uncheckedBrush = new SolidBrush(uncheckedColor);
            totalPageCount = pageCount;
            currntPageIndex = checkedPageIndex;
        }

        private void DrawCheckedCircle(float centerX, float centerY, float radius)
        {
            DrawCircle(checkedPen, checkedBrush, centerX, centerY, radius);
        }

        private void DrawUncheckedCircle(float centerX, float centerY, float radius)
        {
            DrawCircle(uncheckedPen, uncheckedBrush, centerX, centerY, radius);
        }

        private void DrawCircle(Pen pen, Brush brush, float centerX, float centerY, float radius)
        {
            graphics.FillEllipse(brush, centerX - radius, centerY - radius, radius + radius, radius + radius);
            graphics.DrawEllipse(pen, centerX - radius, centerY - radius, radius + radius, radius + radius);
        }

        private void PageIndicator_Paint(object sender, PaintEventArgs e)
        {
            if (totalPageCount <= 0)
                return;

            float radius = Height / 8;
            float centerY = Height / 2;
            float offsetX = Width / (totalPageCount + 1);
            for (int i = 0; i < totalPageCount; i++)
            {
                float centerX = offsetX * (i + 1);
                if (i == currntPageIndex)
                    DrawCheckedCircle(centerX, centerY, radius);
                else
                    DrawUncheckedCircle(centerX, centerY, radius);
            }
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20;
                return cp;
            }
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
        }
    }
}
