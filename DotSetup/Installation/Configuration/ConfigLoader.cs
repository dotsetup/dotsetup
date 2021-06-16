// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DotSetup.Infrastructure;
using DotSetup.Installation.WinForm;

namespace DotSetup.Installation.Configuration
{
    internal class ConfigLoader
    {
        private readonly ConfigParser configParser;

        public ConfigLoader(string[] args)
        {
            try
            {
                CmdReader.GetReader(args);
                configParser = ConfigParser.GetConfig();
                configParser.Init();
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Error("Configuration error - " + e);
#endif
            }
        }

        internal Dictionary<PageDesign, Form> FormsDictionary(FrmParent frmParent, IFormPageBinder pageBinder)
        {
            if (configParser == null)
                return null;
            List<PageDesign> pagesDesign = configParser.GetPagesDesign();

            Dictionary<PageDesign, Form> frmDictionary = new Dictionary<PageDesign, Form>();

            foreach (PageDesign pageDesign in pagesDesign)
            {
                Form page = pageBinder.GetFormPage(pageDesign.PageName, frmParent);
                if (page != null)
                    frmDictionary.Add(pageDesign, DecoratePage(page, pageDesign));
            }
            return frmDictionary;
        }

        internal void DecorateForms(Dictionary<PageDesign, Form> frmDictionary)
        {
            if (configParser == null)
                return;
            List<PageDesign> pagesDesign = configParser.GetPagesDesign();

            foreach (KeyValuePair<PageDesign, Form> kvp in frmDictionary)
            {
                PageDesign newDesign = pagesDesign.FirstOrDefault(x => x.PageName == kvp.Key.PageName);
                PageDesign oldDesign = kvp.Key;
                if (!string.IsNullOrEmpty(newDesign.PageName) && newDesign.ControlsLayouts.CompareTo(oldDesign.ControlsLayouts) == 1)
                {
                    DecoratePage(kvp.Value, newDesign);
                }
            };
        }

        private Form DecoratePage(Form page, PageDesign pageDesign)
        {
            pageDesign.ControlsLayouts.SetLayout(page.Controls);

            return page;
        }


        public void UpdateParentDesign(Form frmParent)
        {
            if (configParser == null)
                return;
            FormDesign formDesign = configParser.GetFormDesign();
            frmParent.PerformSafely(() =>
            {
                double scalingFactor = frmParent.CreateGraphics().DpiX / 96.0;
                if (formDesign.Height > 0)
                    frmParent.Height = (int)Math.Round(formDesign.Height * scalingFactor);
                if (formDesign.Width > 0)
                    frmParent.Width = (int)Math.Round(formDesign.Width * scalingFactor);
                if (formDesign.ClientWidth > 0 && formDesign.ClientHeight > 0)
                    frmParent.ClientSize = new Size((int)Math.Round(formDesign.ClientWidth * scalingFactor), (int)Math.Round(formDesign.ClientHeight * scalingFactor));

                frmParent.BackColor = formDesign.BackgroundColor;
                if (formDesign.FormName != "")
                {
                    frmParent.Text = formDesign.FormName;
                }
            });

        }
    }
}
