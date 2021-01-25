// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace DotSetup
{
    public class ControlSettings
    {
        protected string cid, value, location, size, backColor, foreColor,
            fontName, fontSize, fontStyle, rtl,
            encode, decode;

        public static class KnownAttribute
        {
            public const string Location = "location", Size = "size", BackColor = "backColor", ForeColor = "foreColor",
            FontName = "fontName", FontSize = "fontSize", FontStyle = "fontStyle", Rtl = "rtl",
            Encode = "encode", Decode = "decode";
        }

        public ControlSettings(string cid, XmlNode node, Dictionary<string, string> defaultAttributes)
        {
            this.cid = cid;
            value = XmlParser.GetStringValue(node);
            Dictionary<string, string> attributes = XmlParser.GetXpathRefAttributes(node);

            location = GetAttributeValue(attributes, defaultAttributes, KnownAttribute.Location);
            size = GetAttributeValue(attributes, defaultAttributes, KnownAttribute.Size);
            backColor = GetAttributeValue(attributes, defaultAttributes, KnownAttribute.BackColor);
            foreColor = GetAttributeValue(attributes, defaultAttributes, KnownAttribute.ForeColor);
            fontName = GetAttributeValue(attributes, defaultAttributes, KnownAttribute.FontName);
            fontSize = GetAttributeValue(attributes, defaultAttributes, KnownAttribute.FontSize);
            fontStyle = GetAttributeValue(attributes, defaultAttributes, KnownAttribute.FontStyle);
            rtl = GetAttributeValue(attributes, defaultAttributes, KnownAttribute.Rtl);
            encode = GetAttributeValue(attributes, defaultAttributes, KnownAttribute.Encode);
            decode = GetAttributeValue(attributes, defaultAttributes, KnownAttribute.Decode);
        }

        private string GetAttributeValue(Dictionary<string, string> attributes, Dictionary<string, string> defaultAttributes, string knownAttribute)
        {
            string res = "";
            if (attributes.ContainsKey(knownAttribute))
                res = attributes[knownAttribute];
            else if ((defaultAttributes != null) && defaultAttributes.ContainsKey(knownAttribute))
                res = defaultAttributes[knownAttribute];

            return res;
        }

        internal virtual void SetLayout(Control control)
        {
            control.Text = value;
            SetLocation(control);
            SetSize(control);
            SetFont(control);
            SetColor(control);
        }

        internal virtual void SetLocation(Control control)
        {
            if (location != "")
            {
                string[] coords = location.Split(',');
                if (coords.Count() != 2)
                    return;
                control.Location = new Point(GetRelativeValue(coords[0], control.Location.X), GetRelativeValue(coords[1], control.Location.Y));
#if DEBUG
                Logger.GetLogger().Info("Update cid " + cid + " location to " + location);
#endif
            }
        }

        internal virtual void SetSize(Control control)
        {
            if (size != "")
            {
                string[] coords = size.Split(',');
                if (coords.Count() != 2)
                    return;
                control.Size = new Size(GetRelativeValue(coords[0], control.Size.Width), GetRelativeValue(coords[1], control.Size.Height));
#if DEBUG
                Logger.GetLogger().Info("Update cid " + cid + " size to " + size);
#endif
            }
        }

        private int GetRelativeValue(string valStr, int baseValue)
        {
            int retValue = baseValue;
            try
            {
                if (valStr[0] == '+' || valStr[0] == '-')
                {
                    char op = valStr[0];
                    valStr = valStr.Substring(1, valStr.Length - 1);
                    retValue = baseValue + ((op == '+') ? 1 : -1) * int.Parse(valStr);
                }
                else
                    retValue = int.Parse(valStr);
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Warning("Updateing cid " + cid + " with " + valStr + " failed: " + e.Message);
#endif
            }
            finally
            {
            }

            return retValue;
        }

        internal virtual void SetFont(Control control)
        {
            if (fontName != "" || fontSize != "" || fontStyle != "")
            {
                if (fontName == "")
                {
                    fontName = control.Font.Name;
                }
                if (fontSize == "")
                {
                    fontSize = control.Font.SizeInPoints.ToString();
                }
                if (fontStyle == "")
                {
                    fontStyle = control.Font.Style.ToString();
                }
                if (fontSize[0] == '+' || fontSize[0] == '-')
                {
                    char op = fontSize[0];
                    fontSize = fontSize.Substring(1, fontSize.Length - 1);
                    fontSize = (control.Font.SizeInPoints + ((op == '+') ? 1 : -1) * float.Parse(fontSize)).ToString();
                    fontSize = fontSize.ToString();
                }

                control.Font = FontManager.GetManager().GetFont(fontName, fontSize, fontStyle);
#if DEBUG
                Logger.GetLogger().Info("Update cid " + cid + " font to " + control.Font.ToString());
#endif
            }
        }

        internal virtual void SetColor(Control control)
        {
            if (!string.IsNullOrEmpty(backColor))
            {
                control.BackColor = ColorTranslator.FromHtml(backColor);
#if DEBUG
                Logger.GetLogger().Info("Update cid " + cid + " BackColor to " + control.BackColor.ToString());
#endif
            }

            if (!string.IsNullOrEmpty(foreColor))
            {
                control.ForeColor = ColorTranslator.FromHtml(foreColor);
#if DEBUG
                Logger.GetLogger().Info("Update cid " + cid + " ForeColor to " + control.ForeColor.ToString());
#endif
            }
        }
    }
}
