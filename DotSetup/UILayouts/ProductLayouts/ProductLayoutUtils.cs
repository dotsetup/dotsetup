using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DotSetup
{
    public static class ProductLayoutUtils
    {
        public static bool isSDK = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                     from type in assembly.GetTypes()
                     where type.Namespace == "DotSetupSDK"
                     select type).Any();

        public static void ResizeBackground(Control parent, Control background, Control bottom)
        {
            var widthPercent = (double)parent.Width / background.BackgroundImage.Width;
            var heightPercent = (double)parent.Height / background.BackgroundImage.Height;
            var heightByWidth = (int)(background.BackgroundImage.Height * widthPercent);
            var heightByHeight = (int)(background.BackgroundImage.Height * heightPercent) - bottom.Height;
            var bottomLocation = bottom.Location.Y;
            background.Height = (heightByWidth > bottomLocation) ? heightByHeight : heightByWidth;
        }

        public static void MoveOptionalBadge(Control parent, Control badge)
        {
            badge.Location = new Point(parent.Width - badge.Width, 0 + badge.Height);
        }
    }
}
