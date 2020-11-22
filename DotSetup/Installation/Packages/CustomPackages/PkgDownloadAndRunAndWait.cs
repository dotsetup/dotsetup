// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

namespace DotSetup.CustomPackages
{
    class PkgDownloadAndRunAndWait : InstallationPackage
    {
        public PkgDownloadAndRunAndWait(string name) : base(name)
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
