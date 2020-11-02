// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Windows.Forms;

namespace DotSetup
{
    public class ConfigValidator
    {
        public ConfigValidator()
        {
            EventManager.GetManager().AddEvent(DotSetupManager.EventName.OnFatalError, HandleConfigFatalError);
        }
        public string Validate()
        {
            string errorMsg = "";
            ConfigParser configParser = ConfigParser.GetConfig();

            foreach (string mandatoryConst in ConfigConsts.ReportMandatory)
            {
                if (String.IsNullOrEmpty(configParser.GetConfigValue(mandatoryConst)))
                    errorMsg += "No config param called " + mandatoryConst + ", ";
            }

            return errorMsg;
        }

        public void ValidOrExit()
        {
            string errorMsg = Validate();
            if (!String.IsNullOrEmpty(errorMsg))
            {
#if DEBUG
                Logger.GetLogger().Fatal("Configuration validation error - " + errorMsg);
#endif
                EventManager.GetManager().DispatchEvent(DotSetupManager.EventName.OnFatalError);
            }
        }

        internal bool HandleConfigFatalError(object sender, EventArgs e)
        {
            if (EventManager.GetManager().EventCount(DotSetupManager.EventName.OnFatalError) == 1)
            {
                MessageBox.Show("Invalid configuration", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            return true;
        }
    }
}
