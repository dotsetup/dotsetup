// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

namespace DotSetup.CustomPackages
{
    class PkgDownloadAndRun : InstallationPackage
    {
        public PkgDownloadAndRun(string name) : base(name)
        {
        }
        public override void HandleDownloadEnded()
        {
            base.HandleDownloadEnded();
            RunDownloadedFile();
        }
    }
}
