// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using DotSetup.Infrastructure;

namespace DotSetup.UILayouts.ControlLayout
{
    public class ControlsLayout : IComparable
    {
        private readonly Dictionary<string, ControlSettings> _controlSettings;
        private Thread _preparingResourcesThread = null;

        public bool ResourcesReady { get; private set; }

        public ControlsLayout(XmlNodeList[] xmlNodeList, Dictionary<string, string> defaultControlDesign)
        {
            _controlSettings = new Dictionary<string, ControlSettings>();
            ResourcesReady = true;
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

                    if (_controlSettings.ContainsKey(cid))
                    {
#if DEBUG
                        Logger.GetLogger().Error("Page control settings contains already cid " + cid);
#endif
                    }
                    else
                        _controlSettings.Add(cid, cntlSettings);
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
                if (_controlSettings.ContainsKey(control.Name))
                {
                    _controlSettings[control.Name].SetLayout(control);
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
                return _controlSettings.Count == otherControls._controlSettings.Count
                    && _controlSettings.Values.SequenceEqual(otherControls._controlSettings.Values) ? 0 : 1;
            else
                return 1;
        }

        public void PrepareResources(CountdownEvent onReady)
        {
            ResourcesReady = false;
            CountdownEvent preparingResources = new CountdownEvent(_controlSettings.Count);
            foreach (KeyValuePair<string, ControlSettings> kvp in _controlSettings)
            {
                kvp.Value.PrepareResources(preparingResources);
            };

            _preparingResourcesThread = new Thread(() =>
            {
                preparingResources.Wait();
                preparingResources.Dispose();
                ResourcesReady = true;
                foreach (KeyValuePair<string, ControlSettings> kvp in _controlSettings)
                {
#if DEBUG
                    Logger.GetLogger().Info($"{kvp.Key} control settings are " + (kvp.Value.IsReady ? "ready" : "not ready"), Logger.Level.MEDIUM_DEBUG_LEVEL);
#endif
                    ResourcesReady &= kvp.Value.IsReady;
                };
                onReady?.Signal();
            })
            {
                IsBackground = true
            };
            _preparingResourcesThread.Start();
        }

        public void StopWaitingForResources()
        {
            if (_preparingResourcesThread != null)
            {
                _preparingResourcesThread.Abort();
            }
        }
    }
}
