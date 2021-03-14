// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

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
        protected readonly Dictionary<string, InstallationPackage> packageDictionary;
        private readonly object progressLock = new object();
        private DateTime progressSampleTime;
        internal ProductLayoutManager productLayoutManager;

        public bool Activated { get; private set; } = false;
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);  //Mark for deletion
        private readonly List<string> productClasses;


        public PackageManager()
        {
            packageDictionary = new Dictionary<string, InstallationPackage>();
            productLayoutManager = new ProductLayoutManager(this);
            dwnldBytesReceived = 0;
            dwnldBytesTotal = 0;
            pkgCompletedCounter = pkgRunningCounter = 0;
            progressSampleTime = DateTime.Now;
            productClasses = new List<string>();
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
                DirectoryInfo dir = new DirectoryInfo(path)
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
#if DEBUG
            catch (IOException e)
#else
            catch (IOException)
#endif
            {
#if DEBUG
                Logger.GetLogger().Info($"Could not mark all the files in {path} for deletion, error: {e}");
#endif
            }
            finally
            {
                try
                {
                    Directory.Delete(path, true);
                }
#if DEBUG
                catch (Exception e)
#else
                catch (Exception)
#endif
                {
#if DEBUG
                    Logger.GetLogger().Info($"Could not fully delete working dir {path}, error: {e}");
#endif
                }
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
            List<ProductSettings> productsSettings = ConfigParser.GetConfig().GetProductsSettings();
            foreach (KeyValuePair<string, InstallationPackage> pkg in packageDictionary)
            {
                if (pkg.Value.InstallationState == InstallationPackage.State.Init || pkg.Value.InstallationState == InstallationPackage.State.Error)
                {
                    ProductSettings prodSettings = productsSettings.FirstOrDefault(prod => prod.Name == pkg.Key);
                    if (!string.IsNullOrEmpty(prodSettings.Name))
                    {
                        pkg.Value.SetDownloadInfo(prodSettings);
                        pkg.Value.SetExtractInfo(prodSettings);

                        string extraParams = ConfigParser.GetConfig().GetConfigValue("EXTRA_PARAMS");
                        prodSettings.RunParams += string.IsNullOrEmpty(extraParams) ? string.Empty : (" " + extraParams);
                        pkg.Value.SetRunInfo(prodSettings);
                        pkgStartedCount += (pkg.Value.Activate()) ? 1 : 0;
                    }
                }
            }
            Activated = true;
            return pkgStartedCount;
        }

        public virtual InstallationPackage CreatePackage(ProductSettings settings)
        {

            InstallationPackage pkg = new InstallationPackage(settings);
            packageDictionary.Add(pkg.Name, pkg);
            return pkg;
        }

        public virtual void DiscardPackage(ProductSettings settings)
        {
            if (packageDictionary.ContainsKey(settings.Name))
                packageDictionary.Remove(settings.Name);
        }

        internal void SetProductsSettings(List<ProductSettings> productsSettings)
        {
            int maxOptionalProducts = ConfigParser.GetConfig().GetIntValue("//RemoteConfiguration/FlowSettings/MaxProducts", int.MaxValue);
            maxOptionalProducts = maxOptionalProducts == -1 ? int.MaxValue : maxOptionalProducts;
            int optionalProducts = 0;

            foreach (ProductSettings prodSettings in productsSettings)
            {
                if (packageDictionary.ContainsKey(prodSettings.Name))
                    continue;

                if (prodSettings.IsOptional && (optionalProducts >= maxOptionalProducts))
                {
#if DEBUG
                    Logger.GetLogger().Info($"[{prodSettings.Name}] product will not be shown since the limit of optional products to show is: {maxOptionalProducts}");
#endif
                    continue;
                }

                if ((prodSettings.PreInstall.RequirementList != null) && (prodSettings.PreInstall.RequirementsList != null))
                {
                    RequirementHandlers reqHandlers = new RequirementHandlers();
#if DEBUG
                    Logger.GetLogger().Info("[" + prodSettings.Name + "] Checking requirements for product:");
#endif
                    ProductSettings tmpProdSettings = prodSettings;

                    bool res = default;
                    if (productClasses.Contains(tmpProdSettings.Class))
                    {
                        res = false;
#if DEBUG
                        Logger.GetLogger().Info($"Class ({tmpProdSettings.Class}) <Exists> [{string.Join(", ", productClasses)}] => False");
#endif
                        tmpProdSettings.PreInstall.UnfulfilledRequirementType = "Class";
                    }
                    else
                    {
                        res = reqHandlers.HandlersResult(ref tmpProdSettings.PreInstall);
                    }

                    if (!res)
                    {
                        DiscardPackage(tmpProdSettings);
                        continue;
                    }
                }

                if (!string.IsNullOrEmpty(prodSettings.Class))
                    productClasses.Add(prodSettings.Class);

                if (prodSettings.IsOptional)
                    optionalProducts++;
                
                InstallationPackage pkg = CreatePackage(prodSettings);
                pkg.HandleProgress = HandleProgressUpdate;
                productLayoutManager.AddProductSettings(prodSettings);
            }

            productLayoutManager.WaitForProductsSettingsControlsResources(ConfigParser.GetConfig().GetIntValue("//Config/" + ConfigConsts.REMOTE_LAYOUTS_RESOURCES_MAX_GRACETIME_MS, 0));
        }

        internal bool HandleInstallerQuit(bool doRunOnClose)
        {
            ProgressBarUpdater.Close();
            foreach (KeyValuePair<string, InstallationPackage> pkg in packageDictionary)
            {
                pkg.Value.Quit(doRunOnClose);
            }
            CleanWorkDir();
            return true;
        }

        internal void DeclinePackage(string name)
        {
            if (packageDictionary.ContainsKey(name))
            {
                InstallationPackage pkgToDecline = packageDictionary[name];
                pkgToDecline.InstallationState = InstallationPackage.State.Skipped;
            }
        }

        internal void ConfirmPackage(string name)
        {
            if (packageDictionary.ContainsKey(name))
            {
                InstallationPackage pkgToConfirm = packageDictionary[name];
                pkgToConfirm.InstallationState = InstallationPackage.State.Confirmed;
            }
        }

        internal void SkipAll(string nameOfCurrentPackage)
        {
            DeclinePackage(nameOfCurrentPackage);

            foreach (var pkg in packageDictionary)
            {
                if (pkg.Value.isOptional && !pkg.Value.Confirmed && pkg.Value.InstallationState != InstallationPackage.State.Skipped)
                {
                    pkg.Value.ErrorMessage = "user skipped all";
                    pkg.Value.InstallationState = InstallationPackage.State.Discard;
                }
            }
        }

        internal void DiscardPackge(string name, string errorMessage)
        {
            if (packageDictionary.ContainsKey(name))
            {
                InstallationPackage pkgToDiscard = packageDictionary[name];
                pkgToDiscard.ErrorMessage = errorMessage;
                pkgToDiscard.InstallationState = InstallationPackage.State.Discard;
            }
        }

        internal InstallationPackage GetPackageByName(string name) => packageDictionary[name];

        internal int GetOptionalsCount()
        {
            int optionalsCount = 0;
            foreach (var pkg in packageDictionary)
                if (pkg.Value.isOptional)
                    optionalsCount++;
            return optionalsCount;
        }

        internal bool Started() => (packageDictionary.Count == pkgRunningCounter + pkgCompletedCounter);

        internal void HandleProgressUpdate(InstallationPackage pkg)
        {
            lock (progressLock)
            {
                if ((pkg.InstallationState > InstallationPackage.State.Init) && (pkg.DwnldBytesOffset > 0))
                {
                    if (!pkg.hasUpdatedTotal)
                    {
                        dwnldBytesTotal += pkg.DwnldBytesTotal;
                        pkg.hasUpdatedTotal = true;
                        pkgRunningCounter++;
                    }

                    dwnldBytesReceived += pkg.DwnldBytesOffset;
                    currentProgress = Math.Round(100.0 * dwnldBytesReceived / dwnldBytesTotal);
                }

                if (pkg.isProgressCompleted && !pkg.isUpdatedProgressCompleted)
                {
                    pkgCompletedCounter++;
                    if (pkg.hasUpdatedTotal)
                        pkgRunningCounter--;
                    pkg.isUpdatedProgressCompleted = true;
#if DEBUG
                    Logger.GetLogger().Info($"[{pkg.Name}] Package progress completed");
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
