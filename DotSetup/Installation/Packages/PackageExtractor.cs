// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using Ionic.Zip;

namespace DotSetup
{
    public class PackageExtractor
    {
        private readonly InstallationPackage installationPackage;

        public PackageExtractor(InstallationPackage installationPackage)
        {
            this.installationPackage = installationPackage;
        }

        public void Extract(string compresedFile, string uncompresedDir)
        {
            try
            {
                ZipFile zipFile = ZipFile.Read(compresedFile);

                // call to ExtractAll assumes none of the entries are password-protected.
                zipFile.ExtractAll(uncompresedDir, ExtractExistingFileAction.OverwriteSilently);
            }
            catch (System.Exception ex)
            {
                installationPackage.OnInstallFailed(ErrorConsts.ERR_EXTRACT_GENERAL, ex.Message);
            }
            installationPackage.HandleExtractEnded();
        }

    }
}
