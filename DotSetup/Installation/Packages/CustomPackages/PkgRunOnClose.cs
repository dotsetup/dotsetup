// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

namespace DotSetup.CustomPackages
{
    class PkgRunOnClose : InstallationPackage
    {

        public PkgRunOnClose(string name) : base(name)
        {
        }
        public override void HandleExtractEnded()
        {
            isProgressCompleted = true;
            base.HandleExtractEnded();
        }

        public override void Quit(bool runOnClose)
        {
            if (runOnClose)
                RunDownloadedFile();
            base.Quit(runOnClose);
        }
    }
}
