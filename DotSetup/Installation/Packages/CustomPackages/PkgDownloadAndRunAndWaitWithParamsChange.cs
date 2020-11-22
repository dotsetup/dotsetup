// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.IO;
using DotSetup.Infrastructure;

namespace DotSetup.CustomPackages
{
    class PkgDownloadAndRunAndWaitWithParamsChange : InstallationPackage
    {
        public PkgDownloadAndRunAndWaitWithParamsChange(string name) : base(name)
        {
            waitForIt = true;
        }

        public override bool Run()
        {
            if (!File.Exists(runFileName))
            {
                if (File.Exists(Path.Combine(extractFilePath, runFileName)))
                    runFileName = Path.Combine(extractFilePath, runFileName);
                else if (FileUtils.GetMagicNumbers(dwnldFileName, 2) != "504b" || !isExtractable)  //"504b" = "PK" (zip)
                    runFileName = dwnldFileName;
                else
                {
                    errorMessage = $"No runnable file found in: {runFileName}, download file name: {dwnldFileName}, extract file path: {extractFilePath}";
                    OnInstallFailed(ErrorConsts.ERR_RUN_GENERAL, errorMessage);
                }                   
            }

            string extraParams = ConfigParser.GetConfig().GetConfigValue("EXTRA_PARAMS");
            runParams += " " + extraParams;
            runner.Run(runFileName, runParams);

            return true;
        }

        public override void HandleDownloadEnded()
        {
            base.HandleDownloadEnded();
            RunDownloadedFile();
        }
    }
}
