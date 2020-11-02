// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace DotSetup
{
    class ImageSettings : ControlSettings
    {
        public ImageSettings(string cid, XmlNode node, Dictionary<string, string> defaultAttributes) : base(cid, node, defaultAttributes)
        {
        }

        internal override void SetLayout(Control control)
        {
            base.SetLayout(control);
            if (control.GetType() == typeof(PictureBoxEx) || control.GetType().IsSubclassOf(typeof(PictureBoxEx)))
                ((PictureBoxEx)control).SetImage(value, decode);
            else if (control.GetType() == typeof(PanelEx) || control.GetType().IsSubclassOf(typeof(PanelEx)))
                ((PanelEx)control).SetImage(value, decode);
        }
    }
}
