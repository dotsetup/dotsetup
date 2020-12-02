// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

namespace DotSetup.CustomPackages
{
    internal class PkgDownloadAndRunAndWaitWithParamsChange : InstallationPackage
    {
        public PkgDownloadAndRunAndWaitWithParamsChange(string name) : base(name)
        {
            waitForIt = true;
        }

        public override void HandleDownloadEnded()
        {
            base.HandleDownloadEnded();
            RunDownloadedFile();
        }
    }
}
