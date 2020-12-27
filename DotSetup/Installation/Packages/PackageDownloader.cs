﻿// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.IO;


namespace DotSetup
{
    internal abstract class PackageDownloader
    {
        protected InstallationPackage installationPackage;

        public PackageDownloader(InstallationPackage installationPackage)
        {
            this.installationPackage = installationPackage;
            //ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = (SecurityProtocolType)0xF00; // Allow variety of protocols to support different clients 
        }
        public abstract bool Download(string downloadLink, string outFilePath);

        public string UpdateFileNameIfExists(string fullPath)
        {
            int count = 1;

            string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            string path = Path.GetDirectoryName(fullPath);
            string newFullPath = fullPath;

            while (File.Exists(newFullPath))
            {
                string tempFileName = string.Format("{0} ({1})", fileNameOnly, count++);
                newFullPath = Path.Combine(path, tempFileName + extension);
            }
            return newFullPath;
        }

        public virtual void Terminate() { }

    }
}
