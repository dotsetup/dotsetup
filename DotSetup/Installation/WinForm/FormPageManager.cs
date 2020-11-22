﻿// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DotSetup
{
    public class FormPageManager
    {
        Dictionary<PageDesign, Form> formsDictionary = new Dictionary<PageDesign, Form>();
        Form currentForm;
        int currntPageIndex;
        FrmParent frmParent;
        IFormPageBinder pageBinder;

        private static FormPageManager instance = null;
        public static FormPageManager GetManager()
        {
            if (instance == null)
                instance = new FormPageManager();
            return instance;
        }

        public FormPageManager()
        {
            currntPageIndex = 0;
        }


        public void Bind(FrmParent frmParent, IFormPageBinder pageBinder)
        {

            this.frmParent = frmParent;
            this.pageBinder = pageBinder;
            DotSetupManager dotSetupManager = DotSetupManager.GetManager();
            formsDictionary = dotSetupManager.configLoader.FormsDictionary(frmParent, pageBinder);
            dotSetupManager.configLoader.UpdateParentDesign(frmParent);
            frmParent.Load += new System.EventHandler(FrmParent_Load);
        }

        public void LoadFormName(string pageName)
        {
            Form nextForm = null;
            foreach (KeyValuePair<PageDesign, Form> kvp in formsDictionary)
            {
                if (kvp.Key.PageName == pageName)
                {
                    nextForm = kvp.Value;
                    break;
                }
            };
            LoadForm(nextForm);
        }


        private void LoadForm(Form nextForm)
        {
            if (nextForm != null)
            {
#if DEBUG
                Logger.GetLogger().Info("Loading FormName " + nextForm.Name);
#endif
                frmParent.PerformSafely(() =>
                {
                    currentForm = nextForm;
                    currentForm.TopLevel = false;
                    currentForm.AutoScroll = true;
                    currentForm.Dock = DockStyle.Fill;
                    frmParent.pnlContent.Controls.Clear();
                    frmParent.pnlContent.Controls.Add(currentForm);

                    currentForm.Show();
                    currentForm.Activate();
                });
                ConfigParser.GetConfig().SetStringValue(SessionDataConsts.ROOT + SessionDataConsts.CURRENT_FORM, currentForm.Name);
            }
        }

        public void LoadNextForm()
        {
            Form nextForm = null;
            if (currentForm != null)
            {
                int nextPageIndex = -1;
                foreach (KeyValuePair<PageDesign, Form> kvp in formsDictionary)
                {
                    if (kvp.Value == currentForm)
                    {
                        nextPageIndex = kvp.Key.Index;
                        break;
                    }
                };
#if DEBUG
                if (nextPageIndex == -1)
                    Logger.GetLogger().Error("No Form called " + currentForm.Name + " found in [" + String.Join(" ", formsDictionary) + " ]");
#endif
                currntPageIndex = nextPageIndex + 1;
            }

            foreach (KeyValuePair<PageDesign, Form> kvp in formsDictionary)
            {
                if (kvp.Key.Index == currntPageIndex)
                {
                    nextForm = kvp.Value;
                    break;
                }
            };

            if (formsDictionary.Count > currntPageIndex)
                LoadForm(nextForm);
        }

        public void SetUserSelectedLocale(string locale)
        {
            ConfigParser.GetConfig().SetUserSelectedLocale(locale);
            RefreshForms();
        }

        public void RefreshForms()
        {
            frmParent.PerformSafely(() =>
            {
                DotSetupManager.GetManager().configLoader.DecorateForms(formsDictionary);
            });
        }

        public void FrmParent_Load(object sender, EventArgs e)
        {
            if (formsDictionary != null)
                LoadNextForm();
            else
                DotSetupManager.GetManager().FinalizeInstaller(false);
        }

        public void SetProductLayouts()
        {
            frmParent.PerformSafely(() =>
            {
                ProductLayoutManager productLayoutManager = DotSetupManager.GetManager().packageManager.productLayoutManager;
                productLayoutManager.AddProductLayouts();
                pageBinder.SetProductLayouts(productLayoutManager);
            });
        }
    }
}