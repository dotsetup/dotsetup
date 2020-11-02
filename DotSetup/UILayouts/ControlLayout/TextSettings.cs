// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace DotSetup
{
    class TextSettings : ControlSettings
    {
        public TextSettings(string cid, XmlNode node, Dictionary<string, string> defaultAttributes) : base(cid, node, defaultAttributes)
        {
        }

        internal override void SetLayout(Control control)
        {
            if (rtl == "true")
            {
                control.RightToLeft = RightToLeft.Yes;
            }
            base.SetLayout(control);
        }
    }
}
