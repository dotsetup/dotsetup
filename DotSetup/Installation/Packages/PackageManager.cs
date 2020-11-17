// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using DotSetup.CustomPackages;

namespace DotSetup
{
    [Flags]
    public enum MoveFileFlags
    {
        DelayUntilReboot = 0x00000004
    }

    public class PackageManager
    {
        private double currentProgress, avgDwnldSpeed;
        private int pkgCompletedCounter, pkgRunningCounter, progressSampleCnt;
        private long dwnldBytesReceived, dwnldBytesTotal, lastDwnldBytesReceived;
        private readonly Dictionary<string, InstallationPackage> packageDictionary;
        private readonly object progressLock = new object();
        private DateTime progressSampleTime;
        internal ProductLayoutManager productLayoutManager;
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);  //Mark for deletion


        public PackageManager()
        {
            packageDictionary = new Dictionary<string, InstallationPackage>();
            productLayoutManager = new ProductLayoutManager(this);
            dwnldBytesReceived = 0;
            dwnldBytesTotal = 0;
            pkgCompletedCounter = pkgRunningCounter = 0;
            progressSampleTime = DateTime.Now;
        }

        private static void SetAttributesNormal(DirectoryInfo dir)
        {
            foreach (var subDir in dir.GetDirectories())
                SetAttributesNormal(subDir);
            foreach (var file in dir.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
            }
        }

        private static void DeleteDirectory(string path)
        {
            try
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(path)
                {
                    Attributes = FileAttributes.Normal
                };
                SetAttributesNormal(dir);
                foreach (string file in Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly))
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    if (File.Exists(file))
                        MoveFileEx(file, null, MoveFileFlags.DelayUntilReboot);
                }
                if (Directory.Exists(path))
                    MoveFileEx(path, null, MoveFileFlags.DelayUntilReboot);
            }
            catch (IOException)
            {
#if DEBUG
                Logger.GetLogger().Info("Could not fully delete working directory: " + path);
#endif
            }
        }

        public static void CleanWorkDir()
        {
            lock (ConfigParser.GetConfig().workDir)
            {
                if (Directory.Exists(ConfigParser.GetConfig().workDir))
                {
                    DeleteDirectory(ConfigParser.GetConfig().workDir);
                }
            }
        }

        internal int Activate()
        {
            int pkgStartedCount = 0;
            foreach (KeyValuePair<string, InstallationPackage> pkg in packageDictionary)
            {
                if (pkg.Value.InstallationState == InstallationPackage.State.Init || pkg.Value.InstallationState == InstallationPackage.State.Error)
                    pkgStartedCount += (pkg.Value.Activate()) ? 1 : 0;
            }
            return pkgStartedCount;
        }

        public InstallationPackage CreatePackage(string productLogic, string name)
        {
            InstallationPackage pkg = CustomPackage.CreateCustomPackage(productLogic, name);
            pkg.handleProgress = HandleProgressUpdate;
            packageDictionary.Add(name, pkg);
            return pkg;
        }

        internal void SetProductsSettings(List<ProductSettings> productsSettings)
        {
            int maxProducts = ConfigParser.GetConfig().GetIntValue("//RemoteConfiguration/FlowSettings/MaxProducts", int.MaxValue);
            maxProducts = maxProducts == -1 ? int.MaxValue : maxProducts;
            int remainAllowedProducts = maxProducts;

            foreach (ProductSettings prodSettings in productsSettings)
            {
                if (packageDictionary.ContainsKey(prodSettings.Name))
                    continue;

                if (prodSettings.IsOptional && remainAllowedProducts == 0)
                {
#if DEBUG
                    Logger.GetLogger().Info("[" + prodSettings.Name + "] product will not be shown since the limit of optional products to show is: " + maxProducts.ToString());
#endif
                    continue;
                }

                if ((prodSettings.PreInstall.RequirementList != null) && (prodSettings.PreInstall.RequirementsList != null))
                {
                    RequirementHandlers reqHandlers = new RequirementHandlers();
#if DEBUG
                    Logger.GetLogger().Info("[" + prodSettings.Name + "] Checking requirements for product:");
#endif
                    bool res = reqHandlers.HandlersResult(prodSettings.PreInstall);

                    if (!res)
                        continue;

                    remainAllowedProducts--;
                }

                InstallationPackage pkg = CreatePackage(prodSettings.Behavior, prodSettings.Name);
                pkg.SetDownloadInfo(prodSettings.DownloadURLs, InstallationPackage.ChooseDownloadFileName(prodSettings));
                pkg.SetExtractInfo(prodSettings.ExtractPath);
                pkg.SetRunInfo(prodSettings.RunPath, prodSettings.RunParams, prodSettings.MsiTimeoutMS);
                pkg.SetOptional(prodSettings.IsOptional);
                productLayoutManager.AddProductSettings(prodSettings);
            }
        }

        internal Boolean HandleInstallerQuit(bool doRunOnClose)
        {

            foreach (KeyValuePair<string, InstallationPackage> pkg in packageDictionary)
            {
                pkg.Value.Quit(doRunOnClose);
            }
            CleanWorkDir();
            return true;
        }

        internal void DeclinePackge(string name)
        {
            if (packageDictionary.ContainsKey(name))
            {
                InstallationPackage pkgToDecline = packageDictionary[name];
                pkgToDecline.ChangeState(InstallationPackage.State.Skipped);
            }
        }

        internal void SkipAll()
        {
            foreach (var pkg in packageDictionary)
            {
                if (pkg.Value.isOptional)
                {
                    pkg.Value.ChangeState(InstallationPackage.State.Skipped);
                }
            }
        }

        internal int GetOptionalsCount()
        {
            int optionalsCount = 0;
            foreach (var pkg in packageDictionary)
                if (pkg.Value.isOptional)
                    optionalsCount++;
            return optionalsCount;
        }

        internal void HandleProgressUpdate(InstallationPackage pkg)
        {
            lock (progressLock)
            {
                if ((pkg.InstallationState > InstallationPackage.State.Init) && (pkg.dwnldBytesOffset > 0))
                {
                    if (!pkg.hasUpdatedTotal)
                    {
                        dwnldBytesTotal += pkg.dwnldBytesTotal;
                        pkg.hasUpdatedTotal = true;
                        pkgRunningCounter++;
                    }

                    dwnldBytesReceived += pkg.dwnldBytesOffset;
                    currentProgress = Math.Round(100.0 * dwnldBytesReceived / dwnldBytesTotal);
                }

                if (pkg.isProgressCompleted && !pkg.isUpdatedProgressCompleted)
                {
                    pkgCompletedCounter++;
                    if (pkg.hasUpdatedTotal)
                        pkgRunningCounter--;
                    pkg.isUpdatedProgressCompleted = true;
#if DEBUG
                    Logger.GetLogger().Info(String.Format("[{0}] Package Progress completed", pkg.name));
#endif
                }

                if (packageDictionary.Count > pkgRunningCounter + pkgCompletedCounter)
                    return;

                if ((currentProgress > 100) || (packageDictionary.Count == pkgCompletedCounter))
                    currentProgress = 100;

                if (((DateTime.Now - progressSampleTime).TotalMilliseconds < 100) && (currentProgress != 100))
                    return;

                progressSampleCnt = (progressSampleCnt < 50) ? progressSampleCnt + 1 : 1;
                avgDwnldSpeed = avgDwnldSpeed * (progressSampleCnt - 1) / progressSampleCnt + CalcCurrentDownloadSpeed(dwnldBytesReceived) / progressSampleCnt;

                progressSampleTime = DateTime.Now;
                ProgressEventArgs progressEvent = new ProgressEventArgs("", Convert.ToInt32(currentProgress), 
                    dwnldBytesReceived, dwnldBytesTotal, avgDwnldSpeed, packageDictionary.Count == pkgCompletedCounter);

                ProgressBarUpdater.HandleProgress(progressEvent);
            }
        }

        internal int GetCurrentProgress()
        {
            return Convert.ToInt32(currentProgress);
        }

        private double CalcCurrentDownloadSpeed(long dwnldBytesReceived)
        {
            double currentDwnldSpeed = 0;
            double millisecondsPassed = (DateTime.Now - progressSampleTime).TotalMilliseconds;
            if (dwnldBytesReceived > 0 && millisecondsPassed > 0)
            {
                long dwnldBitsReceived = dwnldBytesReceived * 8;
                if (lastDwnldBytesReceived > 0)
                {
                    currentDwnldSpeed = (dwnldBitsReceived - lastDwnldBytesReceived) / millisecondsPassed;
                }
                lastDwnldBytesReceived = dwnldBitsReceived;
            }

            return currentDwnldSpeed;
        }
    }
}
