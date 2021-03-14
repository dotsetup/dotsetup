// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using DotSetup.UILayouts.UIComponents;

namespace DotSetup
{
    internal class ImageSettings : ControlSettings
    {
        private Image _backgroundImage = null;
        public ImageSettings(string cid, XmlNode node, Dictionary<string, string> defaultAttributes) : base(cid, node, defaultAttributes)
        {
        }

        internal override void SetLayout(Control control)
        {
            base.SetLayout(control);
            if ((control is PictureBoxEx pbx) && !(string.IsNullOrWhiteSpace(value)))
                pbx.SetImage(value, decode);
            else if (control is PanelEx pnlx)
            {
                if (_backgroundImage != null)
                    pnlx.BackgroundImage = _backgroundImage;
                else
                    pnlx.SetImage(value, decode);
            }
        }

        internal override void PrepareResources(CountdownEvent onReady)
        {
            IsReady = false;
            new Thread(() =>
            {
                _backgroundImage = UICommon.PrepareImage(value, decode);                
                if (_backgroundImage != null)
                    IsReady = true;
                onReady?.Signal();
            }).Start();
        }        
    }
}
