// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DotSetup.Infrastructure;

/**
 * Package class represents a single installation component which can be downloaded and treated in a completely autonomic way. 
 * Installation may consist of several packages. During its life-cycle Package goes through several stages: download, extraction 
 * and run. Any package should be reported as complete using one of the report...() methods.
 * This is done to allow the developer to accomplish all the required manual settings for the package and verify that the package 
 * installation was indeed successful.
 * Using packages provides the developer with maximum flexibility in terms of logic, network connection utilization, data 
 * maintenance and more. Intelligent usage of packages can reduce the download size if some of the installation components are already 
 * installed. Another advantage in using packages, is the ability to decide, during start up, which 
 * optional product it the most suitable for the current user and download the right package accordingly. 
 * Package download is boosted by a built-in download accelerator. In order to take full advantage of the accelerator it is
 * recommended to use multiple download sources. Multiple sources assist to achieve higher download speed by utilizing 
 * the entire connection's capacity and also for better fault tolerance if one of the servers is unavailable. The data residing on
 * all the sources must be identical, since the download takes place from several sources in parallel and minimal differences may 
 * damage the data integrity.
*/
namespace DotSetup
{
    public class InstallationPackage
    {
        public static class State
        {
            public const int Error = -1, Skipped = -3, Init = 0, DownloadStart = 1, DownloadEnd = 2, ExtractStart = 3, ExtractEnd = 4, RunStart = 5, RunFinish = 6, Done = 7, Confirmed = 100;
            public static string ToString(int state)
            {
                switch (state)
                {
                    case -1:
                        return "ERROR";
                    case -3:
                        return "SKIPPED";
                    case 0:
                        return "INIT";
                    case 1:
                        return "DOWNLOAD_START";
                    case 2:
                        return "DOWNLOAD_END";
                    case 3:
                        return "EXTRACT_START";
                    case 4:
                        return "EXTRACT_END";
                    case 5:
                        return "RUN_START";
                    case 6:
                        return "RUN_FINISH";
                    case 7:
                        return "DONE";
                    case 100:
                        return "CONFIRMED";
                    default:
                        return "";
                }
            }
        }
        public int PackageState { get; private set; }
        internal int dwnldProgress; //extractProgress, RunProgress;
        internal long dwnldBytesReceived, dwnldBytesOffset, dwnldBytesTotal;
        internal string name, dwnldLink, dwnldFileName, extractFilePath, runFileName, errorMessage, runParams;
        internal bool hasUpdatedTotal, canReport, IsOptional, canRun, isProgressCompleted, isUpdatedProgressCompleted;
        internal int msiTimeout;
        internal Dictionary<string, string> PkgParams;
        internal Action<InstallationPackage> handleProgress;
        internal PackageDownloader downloader;
        internal PackageExtractor extractor;
        internal PackageRunner runner;

        public InstallationPackage(string name)
        {
            errorMessage = "";
            this.name = name;
            downloader = new PackageDownloaderBits(this);
            extractor = new PackageExtractor(this);
            runner = new PackageRunner(this);
            PackageState = State.Init;

            PkgParams = new Dictionary<string, string>();
        }

        internal bool DoRun()
        {
            errorMessage = "";
            PackageState = State.DownloadStart;
#if DEBUG
            Logger.GetLogger().Info("[" + name + "] start downloading from " + dwnldLink + " to location " + dwnldFileName);
#endif

            return downloader.Download(dwnldLink, dwnldFileName);
        }

        public virtual void HandleDownloadEnded()
        {
            FileStream fop;

            try
            {
                SetDownloadProgress(100);
                using (fop = File.OpenRead(dwnldFileName))
                {
                    ChangeState(State.DownloadEnd);

                    if (FileUtils.GetMagicNumbers(dwnldFileName, 2) == "504b")  //"504b" = "PK" (zip)
                    {
                        if (String.IsNullOrEmpty(extractFilePath))
                            extractFilePath = Path.GetDirectoryName(dwnldFileName);
                        extractor.Extract(dwnldFileName, extractFilePath);
                        if (File.Exists(dwnldFileName))
                            fop.Close();
                    }
                    else
                        HandleExtractEnded();
                }
            }
#if DEBUG
            catch (IOException e)
            {
                Logger.GetLogger().Warning("[" + name + "] Cannot opening " + dwnldFileName + " error message: " + e.Message);
            }
#endif
            finally
            {
            }
        }


        public virtual void HandleExtractEnded()
        {
            ChangeState(State.ExtractEnd);
            if (canRun)
                Run();
        }

        public virtual void HandleRunFinished()
        {
            ChangeState(State.RunFinish);
            ChangeState(State.Done);
        }
        public virtual void Quit(bool doRunOnClose)
        {
            downloader.Terminate();
            runner.Terminate();
        }

        public void SetDownloadInfo(List<ProductSettings.DownloadURL> downloadLinks, string downloadFile)
        {
            dwnldLink = String.Empty;
            foreach (ProductSettings.DownloadURL dwnldURL in downloadLinks)
            {

                if ((dwnldURL.Arch == "32" && OSUtils.Is64BitOperatingSystem()) ||
                    (dwnldURL.Arch == "64" && !OSUtils.Is64BitOperatingSystem()) ||
                    (String.IsNullOrEmpty(dwnldURL.URL)))
                    continue;

                dwnldLink = dwnldURL.URL;
                break;
            }

            string potentialFileName = dwnldLink.Substring(dwnldLink.LastIndexOf('/') + 1);

            if (!potentialFileName.Contains("."))
            {
                Random rnd = new Random();
                potentialFileName = rnd.Next(0, Int32.MaxValue).ToString() + ".tmp";
            }

            if (!Path.HasExtension(downloadFile))
                dwnldFileName = Path.Combine(downloadFile, potentialFileName);
            else
                dwnldFileName = downloadFile;
        }

        public void SetExtractInfo(string compressedFilePath)
        {
            extractFilePath = compressedFilePath;
            if (!String.IsNullOrEmpty(extractFilePath) && !ResourcesUtils.IsPathDirectory(compressedFilePath))
                SetErrorMessage("Run path not valid: " + compressedFilePath);
        }

        public void SetRunInfo(string fileName, string runParams, int msiTimeout)
        {
            runFileName = fileName;
            if (!String.IsNullOrEmpty(fileName) && ResourcesUtils.IsPathDirectory(fileName))
                SetErrorMessage("Run path not valid: " + fileName);

            if (msiTimeout == 0)
                msiTimeout = 120000; //2 min default

            this.msiTimeout = msiTimeout;
            this.runParams = runParams;
        }

        public virtual bool Run(bool waitForIt = false)
        {
            if (!File.Exists(runFileName))
            {
                if (File.Exists(Path.Combine(extractFilePath, runFileName)))
                    runFileName = Path.Combine(extractFilePath, runFileName);
                else if (dwnldFileName.EndsWith(".exe") || (dwnldFileName.EndsWith(".msi")))
                    runFileName = dwnldFileName;
                else
                    OnInstallFailed(ErrorConsts.ERR_RUN_GENERAL, "No runnable file found in: " + runFileName + ", download file name: " + dwnldFileName + ", extract file path: " + extractFilePath);
            }
            runner.Run(runFileName, runParams, waitForIt);

            return true;
        }
        public void RunDownloadedFile(bool waitForIt = false)
        {
            canRun = true;
            if (PackageState == State.ExtractEnd)
                Run(waitForIt);
        }

        /**
        * Sets package's status to failed. Sometimes when the packages fails during download or extraction, this method can be 
        * called internally 
        */
        public virtual void OnInstallFailed(int ErrCode, string ErrMsg = "")
        {
            SetErrorMessage(ErrMsg);
            ChangeState(State.Error);
        }

        internal void SetDownloadProgress(int progressPercentage, long bytesReceived = -1, long totalBytes = -1)
        {
            dwnldProgress = progressPercentage;
            if (bytesReceived > 0 && bytesReceived >= dwnldBytesReceived)
            {
                dwnldBytesOffset = bytesReceived - dwnldBytesReceived;
                dwnldBytesReceived = bytesReceived;
            }

            if (totalBytes > 0 && dwnldBytesTotal != totalBytes)
                dwnldBytesTotal = totalBytes;

            if (dwnldProgress == 100)
            {
                dwnldBytesReceived = dwnldBytesTotal;
            }
        }

        internal void SetErrorMessage(string errorMessage)
        {
            this.errorMessage = errorMessage;
#if DEBUG
            Logger.GetLogger().Error("[" + name + "] " + errorMessage);
#endif
        }

        public void ChangeState(int pkgState)
        {
            if (PackageState >= State.Init && PackageState != State.Done)
            {
#if DEBUG
                Logger.GetLogger().Info("[" + name + "] Package switching from PackageState " + PackageState + "(" + State.ToString(PackageState) +
                    ") to PackageState " + pkgState + "(" + State.ToString(pkgState) + ")");
#endif
                PackageState = pkgState;
                if (pkgState < State.Init || pkgState == State.Done)
                    isProgressCompleted = true;
                if (pkgState == State.Error && !this.IsOptional)
                {
                    EventManager.GetManager().DispatchEvent(DotSetupManager.EventName.OnFatalError, this);
                }
                handleProgress?.Invoke(this);
            }
        }

        public static string ChooseDownloadFileName(ProductSettings settings)
        {
            string res = settings.Filename;
            if (!String.IsNullOrEmpty(res) && !Path.HasExtension(res))
                res += ".exe";
            if (!(Path.IsPathRooted(res) && !Path.GetPathRoot(res).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)))
            {
                if (!settings.IsOptional)
                {
                    try
                    {
                        res = Path.Combine(KnownFolders.GetPath(KnownFolder.Downloads), res);
                    }
#if DEBUG
                    catch (Exception e)
#else
                    catch (Exception)
#endif
                    {
#if DEBUG
                        Logger.GetLogger().Warning("Cannot find downloads folder: " + e.Message);
#endif
                        res = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), res);
                    }
                }
                else
                {
                    res = Path.Combine(ConfigParser.GetConfig().workDir, res);
                }
            }

            return res;
        }
    }
}
