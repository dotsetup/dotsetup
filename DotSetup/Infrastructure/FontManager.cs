// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using DotSetup.Installation.Configuration;

namespace DotSetup.Infrastructure
{
    public class FontManager
    {
        private readonly PrivateFontCollection pfc;
        private readonly string tempFontsFolder;
        private readonly Dictionary<string, FontFamily> fontFamilyCash;
        private static FontManager _fontManager = null;

        public static FontManager GetManager()
        {
            if (_fontManager == null)
                _fontManager = new FontManager();
            return _fontManager;
        }

        public FontManager()
        {
            pfc = new PrivateFontCollection();
            fontFamilyCash = new Dictionary<string, FontFamily>();
            tempFontsFolder = Path.Combine(ConfigParser.GetConfig().workDir, "Fonts");
        }

        private bool IsFontInstalled(string fontName)
        {
            bool res;
            Font testFont = new Font(fontName, 8);
            res = 0 == string.Compare(fontName, testFont.Name, StringComparison.InvariantCultureIgnoreCase);
            return res;
        }

        private bool IsFontEmbeddedResource(string resourceName)
        {
            return ResourcesUtils.EmbeddedResourceExists(null, resourceName);
        }

        private void LoadEmbeddedResourceFont(string resourceName)
        {
            string fileName = tempFontsFolder + resourceName;
            if (ResourcesUtils.WriteResourceToFile(resourceName, fileName))
                pfc.AddFontFile(fileName);
        }

        public Font GetFont(string fontName, string fontSizeStr, string fontStyleStr)
        {
            FontFamily fontFamily = null;
            float fontSize = float.Parse(fontSizeStr.Replace("pt", ""));
            FontStyle fontStyle = (FontStyle)Enum.Parse(typeof(FontStyle), fontStyleStr, true);

            if (fontFamilyCash.ContainsKey(fontName))
                fontFamily = fontFamilyCash[fontName];
            else
            {
                if (IsFontEmbeddedResource(fontName + ".ttf"))
                {
                    LoadEmbeddedResourceFont(fontName + ".ttf");
                    fontFamily = pfc.Families.FirstOrDefault(ff => ff.Name == fontName);
                }

                if (fontFamily == null && IsFontInstalled(fontName))
                    fontFamily = new FontFamily(fontName);

                if (!fontFamilyCash.ContainsKey(fontName))
                    fontFamilyCash[fontName] = fontFamily;
            }

            Font res;
            if (fontFamily != null)
                res = new Font(fontFamily, fontSize, fontStyle);
            else
                res = new Font(fontName, fontSize, fontStyle);
            return res;
        }
    }
}
