// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

namespace DotSetup.CustomPackages
{
    internal class CustomPackage
    {
        public static InstallationPackage CreateCustomPackage(string productLogic, string name)
        {
            InstallationPackage pkg;
            if (string.IsNullOrEmpty(productLogic))
                pkg = new InstallationPackage(name);
            else if (typeof(PkgDownloadAndRun).Name.Equals(productLogic))
                pkg = new PkgDownloadAndRun(name);
            else if (typeof(PkgDownloadAndRunAndWait).Name.Equals(productLogic))
                pkg = new PkgDownloadAndRunAndWait(name);
            else if (typeof(PkgDownloadAndRunAndWaitWithParamsChange).Name.Equals(productLogic))
                pkg = new PkgDownloadAndRunAndWaitWithParamsChange(name);
            else if (typeof(PkgRunOnClose).Name.Equals(productLogic))
                pkg = new PkgRunOnClose(name);
            else
                pkg = new InstallationPackage(name);
            return pkg;
        }
    }
}
