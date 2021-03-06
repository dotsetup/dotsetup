﻿// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.IO;
using System.Threading;
using DotSetup.Installation.Configuration;
using Shell32;

namespace DotSetup.Installation.Packages
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
                UnZipFromMTAThread(compresedFile, uncompresedDir);
            }
            catch (Exception e)
            {
                installationPackage.ErrorMessage = e.Message;
                installationPackage.OnInstallFailed(ErrorConsts.ERR_EXTRACT_GENERAL, installationPackage.ErrorMessage);
            }
            installationPackage.HandleExtractEnded();
        }

        /// <summary>
        /// Ugh! SSIS runs script tasks on MTA threads but Shell32 only wants to 
        /// run on STA thread. So start a new STA thread to call UnZip, block 
        /// till it's done, then return. 
        /// We use Shell32 since .net 2 doesn't have ZipFile and we prefer not to 
        /// ship other dlls as they normally need to be deployed to the GAC. So this 
        /// is easiest, although not very pretty.
        /// </summary>
        /// <param name="zipFile">File to unzip</param>
        /// <param name="folderPath">Folder to put the unzipped files</param>
        private static void UnZipFromMTAThread(string zipFile, string folderPath)
        {
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                UnZip(zipFile, folderPath);
            }
            else
            {
                // Local variable to hold the caught exception until the caller can rethrow
                Exception unzipException = null;
                ThreadStart staThreadStart = new ThreadStart(() =>
                {
                    try
                    {
                        UnZip(zipFile, folderPath);
                    }
                    catch (Exception ex)
                    {
                        unzipException = ex;
                    }
                });

                Thread staThread = new Thread(staThreadStart);
                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start();
                staThread.Join();
                if (unzipException != null) throw unzipException;
            }
        }

        /// <summary>
        /// From http://www.fluxbytes.com/csharp/unzipping-files-using-shell32-in-c/ but with 
        /// args packed in object array so can be called from new STA Thread in UnZipFromMTAThread().
        /// </summary>
        /// <param name="param">object array containing: [string zipFile, string destinationFolderPath]</param>
        private static void UnZip(string zipFile, string folderPath)
        {
            if (!File.Exists(zipFile))
                throw new FileNotFoundException();

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // implemented according to https://stackoverflow.com/a/26717274
            // using reflection, rather than referencing specific types with specific versions\libraries
            Type shellAppType = Type.GetTypeFromProgID("Shell.Application");
            object objShell = Activator.CreateInstance(shellAppType);

            Folder destinationFolder = (Folder)shellAppType.InvokeMember("NameSpace", System.Reflection.BindingFlags.InvokeMethod, null, objShell, new[] { folderPath });
            Folder sourceFile = (Folder)shellAppType.InvokeMember("NameSpace", System.Reflection.BindingFlags.InvokeMethod, null, objShell, new[] { zipFile });

            FolderItems items = sourceFile.Items();
            // Flags are: No progress displayed, Respond with 'Yes to All' for any dialog, no UI on error
            // I added 1024 although not sure it's relevant with Zip files. 
            // See https://msdn.microsoft.com/en-us/library/windows/desktop/bb787866%28v=vs.85%29.aspx?f=255&MSPPError=-2147217396
            destinationFolder.CopyHere(items, 4 | 16 | 1024);
        }

    }
}
