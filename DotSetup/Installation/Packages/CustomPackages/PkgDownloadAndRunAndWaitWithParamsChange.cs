// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.IO;

namespace DotSetup.CustomPackages
{
    class PkgDownloadAndRunAndWaitWithParamsChange : InstallationPackage
    {
        public PkgDownloadAndRunAndWaitWithParamsChange(string name) : base(name)
        {
        }

        public override bool Run(bool waitForIt = false)
        {
            if (!File.Exists(runFileName))
            {
                if (File.Exists(Path.Combine(extractFilePath, runFileName)))
                    runFileName = Path.Combine(extractFilePath, runFileName);
                else if (dwnldFileName.EndsWith(".exe") || (dwnldFileName.EndsWith(".msi")))
                    runFileName = dwnldFileName;
                else
                    OnInstallFailed(ErrorConsts.ERR_RUN_GENERAL, "No runnable file found in: " + runFileName + ", download file name: " + dwnldFileName + ", extract file path: " + extractFilePath);
            }

            string extraParams = ConfigParser.GetConfig().GetConfigValue("EXTRA_PARAMS");
            runParams += " " + extraParams;
            runner.Run(runFileName, runParams, waitForIt);

            return true;
        }

        public override void HandleDownloadEnded()
        {
            base.HandleDownloadEnded();
            RunDownloadedFile(true);
        }
    }
}
