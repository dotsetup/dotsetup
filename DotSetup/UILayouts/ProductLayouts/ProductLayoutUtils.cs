// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DotSetup.Infrastructure;
using DotSetup.UILayouts.UIComponents;

namespace DotSetup.UILayouts.ProductLayouts
{
    public static class ProductLayoutUtils
    {
        public static bool isSDK = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                    from type in assembly.GetTypes()
                                    where type.Namespace == "DotSetupSDK"
                                    select type).Any();

        public static void ResizeBackground(Control parent, Control background, Control bottom)
        {
            try
            {
                int imageWidth;
                int imageHeight;

                if (background is PictureBox)
                {
                    imageWidth = background.Width;
                    imageHeight = background.Height;
                }
                else
                {
                    imageWidth = background.BackgroundImage.Width;
                    imageHeight = background.BackgroundImage.Height;
                }

                double widthPercent = (double)parent.Width / imageWidth;
                double heightPercent = (double)parent.Height / imageHeight;
                int heightByWidth = (int)(imageHeight * widthPercent);
                int heightByHeight = (int)(imageHeight * heightPercent) - bottom.Height;
                background.Height = heightByWidth > bottom.Location.Y ? heightByHeight : heightByWidth;
            }

#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Error("Failed to resize bg " + e.Message);
#endif
            }
        }

        public static void MoveOptionalBadge(Control parent, Control badge)
        {
            if (isSDK)
                badge.Location = new Point(parent.Width - badge.Width, 10);
            else
                badge.Visible = false;
        }

        public static void MoveDisclaimer(Control parent, RichTextBoxEx disclaimer)
        {
            double lines = disclaimer.GetLineFromCharIndex(disclaimer.Text.Length - 1) + 1.0;
            int newHeight = (int)(lines * (disclaimer.Font.Height + disclaimer.Font.SizeInPoints - 5));
            disclaimer.Location = new Point(0, parent.Height - newHeight);
            disclaimer.Size = new Size(parent.Width, newHeight);
        }

        public static void SetFontSize(Control parent, int width, RichTextBoxEx title, RichTextBoxEx description)
        {
            if (width == 0)
                return;
            var percent = (float)parent.Width / width;

            if (percent == 0)
                return;

            title.Font = new Font(title.Font.FontFamily, (float)(title.Font.SizeInPoints * percent));
            title.Width = (int)(title.Width * percent);
            title.LineSpacing = (int)(title.LineSpacing * percent);
            description.Font = new Font(description.Font.FontFamily, (float)(description.Font.SizeInPoints * percent));
            description.LineSpacing = (int)(description.LineSpacing * percent);

            title.RefreshStyle();
            description.RefreshStyle();
        }
    }
}
