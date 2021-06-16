// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DotSetup.Infrastructure;
using DotSetup.Installation.Configuration;
using DotSetup.Installation.Events;
using DotSetup.Installation.WinForm;
using DotSetup.UILayouts.ProductLayouts;

namespace DotSetup.Installation.Packages
{
    [Flags]
    public enum MoveFileFlags
    {
        DelayUntilReboot = 0x00000004
    }

    public class PackageManager
    {
        private double currentProgress, avgDwnldSpeed;
        private int pkgCompletedCounter, pkgRunningCounter, progressSampleCnt, _pkgConfirmedCounter, _maxConfirmedPackages;
        private long dwnldBytesReceived, dwnldBytesTotal, lastDwnldBytesReceived;
        protected readonly Dictionary<string, InstallationPackage> _packageDictionary;
        private readonly object progressLock = new object();
        private DateTime progressSampleTime;
        internal ProductLayoutManager productLayoutManager;
        private string _passedExclusive = string.Empty;

        public bool Activated { get; private set; } = false;
        public bool Parsed { get; private set; } = false;
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);  //Mark for deletion
        private readonly List<string> productClasses;


        public PackageManager()
        {
            _packageDictionary = new Dictionary<string, InstallationPackage>();
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
            if (!Parsed)
                ParseProducts();

            int pkgStartedCount = 0;
            List<ProductSettings> productsSettings = ConfigParser.GetConfig().GetProductsSettings();
            foreach (KeyValuePair<string, InstallationPackage> pkg in _packageDictionary)
            {
                if ((pkg.Value.InstallationState >= InstallationPackage.State.Init && pkg.Value.InstallationState < InstallationPackage.State.DownloadStart) 
                    || pkg.Value.InstallationState == InstallationPackage.State.Error)
                {
                    ProductSettings prodSettings = productsSettings.FirstOrDefault(prod => prod.Name == pkg.Key);
                    if (!string.IsNullOrEmpty(prodSettings.Name))
                    {
                        pkg.Value.SetDownloadInfo(prodSettings);
                        pkg.Value.SetExtractInfo(prodSettings);

                        string extraParams = ConfigParser.GetConfig().GetConfigValue("EXTRA_PARAMS");
                        prodSettings.RunParams += string.IsNullOrEmpty(extraParams) ? string.Empty : " " + extraParams;
                        pkg.Value.SetRunInfo(prodSettings);
                        pkgStartedCount += pkg.Value.Activate() ? 1 : 0;
                    }
                }
            }
            Activated = true;
            return pkgStartedCount;
        }

        private void SetPackageStateToDisplayed(string name, int index)
        {
            InstallationPackage pkg = _packageDictionary[name];
            if (pkg == null)
                return;
            pkg.InstallationState = InstallationPackage.State.Displayed;
        }

        internal virtual int ParseProducts()
        {            
            if (Activated)
            {
#if DEBUG
                Logger.GetLogger().Warning($"can't parse after activation");
#endif
                return 0;
            }
            productLayoutManager.OnLayoutShown += SetPackageStateToDisplayed;
            SetProductsSettings(ConfigParser.GetConfig().GetProductsSettings());
            Parsed = true;
            return _packageDictionary.Count();
        }

        public virtual InstallationPackage CreatePackage(ProductSettings settings)
        {
            InstallationPackage pkg = new InstallationPackage(settings);            
            return pkg;
        }       

        internal void SetProductsSettings(List<ProductSettings> productsSettings)
        {
            int maxOptionalProducts = ConfigParser.GetConfig().GetIntValue("//RemoteConfiguration/FlowSettings/MaxProducts", int.MaxValue);
            maxOptionalProducts = maxOptionalProducts == -1 ? int.MaxValue : maxOptionalProducts;
            int optionalProducts = 0;

            _maxConfirmedPackages = ConfigParser.GetConfig().GetIntValue("//RemoteConfiguration/FlowSettings/MaxAcceptedProducts", int.MaxValue);
            _maxConfirmedPackages = _maxConfirmedPackages == -1 ? int.MaxValue : _maxConfirmedPackages;
            _pkgConfirmedCounter = 0;

            foreach (ProductSettings prodSettings in productsSettings)
            {
                if (_packageDictionary.ContainsKey(prodSettings.Name))
                    continue;

                if (prodSettings.IsOptional && optionalProducts >= maxOptionalProducts)
                {
#if DEBUG
                    Logger.GetLogger().Info($"[{prodSettings.Name}] product will not be shown since the limit of optional products to show is: {maxOptionalProducts}");
#endif
                    continue;
                }

                InstallationPackage pkg = CreatePackage(prodSettings);
                pkg.InstallationState = InstallationPackage.State.CheckStart;

                if (prodSettings.PreInstall.RequirementList != null && prodSettings.PreInstall.RequirementsList != null)
                {
                    RequirementHandlers reqHandlers = new RequirementHandlers();
#if DEBUG
                    Logger.GetLogger().Info($"[{prodSettings.Name}] Checking requirements for product:");
#endif
                    ProductSettings tmpProdSettings = prodSettings;

                    bool res = false;
                    if (tmpProdSettings.Exclusive && optionalProducts > 0)
                    {

                        string dictionaryString = "{";
                        foreach (KeyValuePair<string, InstallationPackage> keyValues in _packageDictionary)
                        {
                            if (keyValues.Value.isOptional)
                                dictionaryString += keyValues.Key + ", ";
                        }                            

                        dictionaryString = dictionaryString.TrimEnd(',', ' ') + "}";
#if DEBUG
                        Logger.GetLogger().Info($"Exclusive {dictionaryString} <Equal> [] => False");
#endif

                        tmpProdSettings.PreInstall.UnfulfilledRequirementType = "Exclusive";
                        tmpProdSettings.PreInstall.UnfulfilledRequirementDelta = dictionaryString;
                    }
                    else if (!string.IsNullOrEmpty(_passedExclusive))
                    {
#if DEBUG
                        Logger.GetLogger().Info($"Exclusive ({_passedExclusive}) <Exists> [] => False");
#endif
                        tmpProdSettings.PreInstall.UnfulfilledRequirementType = "Exclusive";
                        tmpProdSettings.PreInstall.UnfulfilledRequirementDelta = $"Exclusive product {_passedExclusive} already passed";
                    }
                    else if (productClasses.Contains(tmpProdSettings.Class))
                    {
#if DEBUG
                        Logger.GetLogger().Info($"Class ({tmpProdSettings.Class}) <Exists> [{string.Join(", ", productClasses)}] => False");
#endif
                        tmpProdSettings.PreInstall.UnfulfilledRequirementType = "Class";
                        tmpProdSettings.PreInstall.UnfulfilledRequirementDelta = $"product of class {_passedExclusive} already passed";
                    }
                    else
                    {
                        res = reqHandlers.HandlersResult(ref tmpProdSettings.PreInstall);
                    }

                    if (!res)
                    {
                        ConfigParser.GetConfig().SetProductSettingsXml(tmpProdSettings, 
                            "StaticData/PreInstall/UnfulfilledRequirement/Type", tmpProdSettings.PreInstall.UnfulfilledRequirementType);
                        ConfigParser.GetConfig().SetProductSettingsXml(tmpProdSettings,
                            "StaticData/PreInstall/UnfulfilledRequirement/Delta", tmpProdSettings.PreInstall.UnfulfilledRequirementDelta);
                        pkg.ProdSettings = tmpProdSettings;
                        pkg.InstallationState = InstallationPackage.State.Discard;
                        continue;
                    }
                }

                // if we got here, all requirements passed
                pkg.InstallationState = InstallationPackage.State.CheckPassed;

                if (!string.IsNullOrEmpty(prodSettings.Class))
                    productClasses.Add(prodSettings.Class);

                if (prodSettings.IsOptional)
                    optionalProducts++;

                if (prodSettings.Exclusive)
                    _passedExclusive = prodSettings.Name;

                _packageDictionary.Add(pkg.Name, pkg);
                pkg.HandleProgress = HandleProgressUpdate;
                productLayoutManager.AddProductSettings(prodSettings);
            }

            productLayoutManager.WaitForProductsSettingsControlsResources(ConfigParser.GetConfig().GetIntValue("//Config/" + ConfigConsts.REMOTE_LAYOUTS_RESOURCES_MAX_GRACETIME_MS, 0));
        }

        internal bool HandleInstallerQuit(bool doRunOnClose)
        {
            ProgressBarUpdater.Close();
            foreach (KeyValuePair<string, InstallationPackage> pkg in _packageDictionary)
            {
                pkg.Value.Quit(doRunOnClose);
            }
            CleanWorkDir();
            return true;
        }

        internal void DeclinePackage(string name)
        {
            if (_packageDictionary.ContainsKey(name))
            {
                InstallationPackage pkgToDecline = _packageDictionary[name];
                pkgToDecline.InstallationState = InstallationPackage.State.Skipped;
            }
        }

        public bool MaxConfirmedPackagesReached => _pkgConfirmedCounter >= _maxConfirmedPackages;

        internal void ConfirmPackage(string name)
        {
            if (_packageDictionary.ContainsKey(name))
            {
                _pkgConfirmedCounter++;
                InstallationPackage pkgToConfirm = _packageDictionary[name];
                pkgToConfirm.InstallationState = InstallationPackage.State.Confirmed;
            }

            if (_pkgConfirmedCounter >= _maxConfirmedPackages)
            {
#if DEBUG
                Logger.GetLogger().Info($"{_pkgConfirmedCounter} packages were confirmed which is the maximal number. going to skip all other packages...");
#endif
                SkipAll("");
            }
        }

        internal void SkipAll(string nameOfCurrentPackage)
        {
            if (!string.IsNullOrEmpty(nameOfCurrentPackage))
                DeclinePackage(nameOfCurrentPackage);

            foreach (var pkg in _packageDictionary)
            {
                if (pkg.Value.isOptional && !pkg.Value.Confirmed && pkg.Value.InstallationState != InstallationPackage.State.Skipped)
                {
                    pkg.Value.ErrorMessage = "user skipped all";
                    pkg.Value.InstallationState = InstallationPackage.State.Discard;
                }
            }
        }

        internal void DiscardPackage(string name, string errorMessage)
        {
            if (_packageDictionary.ContainsKey(name))
            {
                InstallationPackage pkgToDiscard = _packageDictionary[name];
                pkgToDiscard.ErrorMessage = errorMessage;
                pkgToDiscard.InstallationState = InstallationPackage.State.Discard;
            }
        }        

        public InstallationPackage GetPackageByName(string name) => _packageDictionary[name];

        internal int GetOptionalsCount()
        {
            int optionalsCount = 0;
            foreach (var pkg in _packageDictionary)
                if (pkg.Value.isOptional)
                    optionalsCount++;
            return optionalsCount;
        }

        internal bool Started() => _packageDictionary.Count == pkgRunningCounter + pkgCompletedCounter;

        internal void HandleProgressUpdate(InstallationPackage pkg)
        {
            lock (progressLock)
            {
                if (pkg.InstallationState >= InstallationPackage.State.DownloadStart && pkg.DwnldBytesOffset > 0)
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

                if (_packageDictionary.Count > pkgRunningCounter + pkgCompletedCounter)
                    return;

                if (currentProgress > 100 || _packageDictionary.Count == pkgCompletedCounter)
                    currentProgress = 100;

                if ((DateTime.Now - progressSampleTime).TotalMilliseconds < 100 && currentProgress != 100)
                    return;

                progressSampleCnt = progressSampleCnt < 50 ? progressSampleCnt + 1 : 1;
                avgDwnldSpeed = avgDwnldSpeed * (progressSampleCnt - 1) / progressSampleCnt + CalcCurrentDownloadSpeed(dwnldBytesReceived) / progressSampleCnt;

                progressSampleTime = DateTime.Now;

                string errorMessage = string.Empty;
                if (pkg.InstallationState == InstallationPackage.State.Error && !pkg.isOptional)
                    errorMessage = pkg.ErrorMessage;

                ProgressEventArgs progressEvent = new ProgressEventArgs(errorMessage, Convert.ToInt32(currentProgress),
                    dwnldBytesReceived, dwnldBytesTotal, avgDwnldSpeed, _packageDictionary.Count == pkgCompletedCounter);

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
