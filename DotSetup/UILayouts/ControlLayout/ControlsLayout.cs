// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace DotSetup
{
    public class ControlsLayout : IComparable
    {
        private readonly Dictionary<string, ControlSettings> controlSettings;

        public ControlsLayout(XmlNodeList[] xmlNodeList, Dictionary<string, string> defaultControlDesign)
        {
            controlSettings = new Dictionary<string, ControlSettings>();
            foreach (XmlNodeList nodeList in xmlNodeList)
            {
                foreach (XmlNode node in nodeList)
                {
                    ControlSettings cntlSettings;
                    string cid = XmlParser.GetStringAttribute(node, "cid");
                    if (node.Name == "Text" || cid.StartsWith("txt"))
                        cntlSettings = new TextSettings(cid, node, defaultControlDesign);
                    else if (node.Name == "Image" || cid.StartsWith("img"))
                        cntlSettings = new ImageSettings(cid, node, defaultControlDesign);
                    else if (node.Name == "Button" || cid.StartsWith("btn"))
                        cntlSettings = new ButtonsSettings(cid, node, defaultControlDesign);
                    else
                        cntlSettings = new ControlSettings(cid, node, defaultControlDesign);

                    if (controlSettings.ContainsKey(cid))
                    {
#if DEBUG
                        Logger.GetLogger().Error("Page control settings contains already cid " + cid);
#endif
                    }
                    else
                        controlSettings.Add(cid, cntlSettings);
                }
            }
        }

        public void SetLayout(Control.ControlCollection controls)
        {
            SetLayout(controls.Cast<Control>());
        }

        public void SetLayout(IEnumerable<Control> controls)
        {
            foreach (Control control in controls)
            {
                if (controlSettings.ContainsKey(control.Name))
                {
                    controlSettings[control.Name].SetLayout(control);
                }

                if (control.HasChildren)
                {
                    // Recursively call this method for each child control.
                    SetLayout(control.Controls.Cast<Control>());
                }
            }
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return -1;

            if (obj is ControlsLayout otherControls)
                return ((controlSettings.Count == otherControls.controlSettings.Count)
                    && controlSettings.Values.SequenceEqual(otherControls.controlSettings.Values)) ? 0 : 1;
            else
                return 1;
        }
    }
}
