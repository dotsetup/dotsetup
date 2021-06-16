// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace DotSetup.UILayouts.ControlLayout
{
    internal class ButtonsSettings : ControlSettings
    {
        public ButtonsSettings(string cid, XmlNode node, Dictionary<string, string> defaultAttributes) : base(cid, node, defaultAttributes)
        {
        }
        internal override void SetLayout(Control control)
        {
            base.SetLayout(control);
        }
    }
}
