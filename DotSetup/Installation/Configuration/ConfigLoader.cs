// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DotSetup
{

    class ConfigLoader
    {
        private readonly ConfigParser configParser;
        private readonly ConfigValidator configValidator;

        public ConfigLoader(string[] args, Boolean ExternalParent)
        {
            try
            {
                configParser = new ConfigParser();
                if (!ExternalParent)
                {
                    configValidator = new ConfigValidator();
                    configValidator.ValidOrExit();
                }
            }
#if DEBUG
            catch (Exception e)
            {
                Logger.GetLogger().Error("Configuration error - " + e);
            }
#endif
            finally
            {
            }
        }


        internal Dictionary<string, Form> FormsDictionary(frmParent frmParent, IFormPageBinder pageBinder)
        {
            if (configParser == null)
                return null;
            List<PageDesign> pagesDesign = configParser.GetPagesDesign();

            Dictionary<string, Form> frmDictionary = new Dictionary<string, Form>();

            for (int i = 0; i < pagesDesign.Count; i++)
            {
                Form page = pageBinder.GetFormPage(pagesDesign[i].PageName, frmParent);
                if (page != null)
                    frmDictionary.Add(pagesDesign[i].PageName, DecoratePage(page, pagesDesign, i));
            }
            return frmDictionary;
        }

        internal List<string> PagesNames()
        {
            List<PageDesign> pagesDesign = configParser.GetPagesDesign();
            return pagesDesign.ConvertAll(x => x.PageName);
        }

        private Form DecoratePage(Form page, List<PageDesign> pagesDesign, int pageIndex)
        {
            PageDesign pageDesign = pagesDesign[pageIndex];

            pageDesign.ControlsLayouts.SetLayout(page.Controls);

            return page;
        }


        internal void UpdateInstallerManager(DotSetupManager installerManager)
        {
            if (configParser != null)
            {
                installerManager.packageManager.SetProductsSettings(configParser.GetProductsSettings());
            }
        }

        private void UpdateParentDesign(frmParent frmParent)
        {
            if (configParser == null)
                return;
            FormDesign formDesign = configParser.GetFormDesign();
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
        }

        public void UpdateParentForm(frmParent frmParent)
        {
            UpdateParentDesign(frmParent);
        }
    }
}
