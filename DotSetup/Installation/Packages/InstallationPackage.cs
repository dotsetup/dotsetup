// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using DotSetup.Infrastructure;

/**
 * Package class represents a single installation component which can be downloaded and treated in a completely autonomic way. 
 * Installation may consist of several packages. During its life-cycle Package goes through several stages: download, extraction 
 * and run.
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
            public const int Error = -1, Skipped = -3, Discard = -4, Init = 0, DownloadStart = 1, DownloadEnd = 2, ExtractStart = 3, ExtractEnd = 4, RunStart = 5, RunEnd = 6, Done = 7, Confirmed = 100;
            public static string ToString(int state)
            {
                return state switch
                {
                    -1 => "ERROR",
                    -3 => "SKIPPED",
                    0 => "INIT",
                    1 => "DOWNLOAD_START",
                    2 => "DOWNLOAD_END",
                    3 => "EXTRACT_START",
                    4 => "EXTRACT_END",
                    5 => "RUN_START",
                    6 => "RUN_END",
                    7 => "DONE",
                    100 => "CONFIRMED",
                    _ => "",
                };
            }
        }
        private int _dwnldProgress;
        private string _secondDownloadMethod;
        public int InstallationState { get; private set; }
        private long _dwnldBytesReceived;
        internal long DwnldBytesOffset { get; private set; }
        private long _dwnldBytesTotal;
        internal long DwnldBytesTotal
        {
            get => _dwnldBytesTotal;
            private set
            {
                if (value > 0)
                    _dwnldBytesTotal = value;
            }
        }
        private string _extractFilePath;
        private string _dwnldFileName;
        internal string RunFileName { get; private set; }
        internal string Name { get; }
        internal string DwnldLink { get; private set; }
        internal string FileHash { get; private set; }
        internal string RunParams { get; private set; }
        internal string ErrorMessage { get; set; }
        internal bool hasUpdatedTotal, isOptional, isExtractable;
        internal bool RunWithBits { get; set; }
        internal bool canRun, firstDownloaded, isProgressCompleted, isUpdatedProgressCompleted, waitForIt;
        internal int msiTimeout, runErrorCode, runExitCode;
        internal Action<int> OnChangeState;
        internal Action OnInstallSuccess, OnUserQuit;
        internal Action<int, string> OnInstallFailed;
        internal Action<InstallationPackage> HandleProgress;
        internal PackageDownloader downloader;
        internal PackageExtractor extractor;
        internal PackageRunner runner;
        internal ManualResetEvent onRunWithBits = new ManualResetEvent(false);

        public InstallationPackage(string name)
        {
            Name = name;
            extractor = new PackageExtractor(this);
            runner = new PackageRunner(this);
            InstallationState = State.Init;
            OnInstallFailed += HandleInstallFailed;
        }

        internal bool Activate()
        {
            InstallationState = State.DownloadStart;
#if DEBUG
            Logger.GetLogger().Info("[" + Name + "] start downloading from " + DwnldLink + " to location " + _dwnldFileName);
#endif

            return downloader.Download(DwnldLink, _dwnldFileName);
        }

        public virtual void HandleDownloadEnded()
        {
            FileStream fop;

            try
            {
                SetDownloadProgress(100);
                using (fop = File.OpenRead(_dwnldFileName))
                {
                    FileHash = CryptUtils.ComputeHash(fop, CryptUtils.Hash.SHA1);

                    ChangeState(State.DownloadEnd);

                    if (FileUtils.GetMagicNumbers(_dwnldFileName, 2) == "504b" && isExtractable)  //"504b" = "PK" (zip)    
                        extractor.Extract(_dwnldFileName, _extractFilePath);
                    else
                        HandleExtractEnded();
                }
            }
#if DEBUG
            catch (IOException e)
#else
            catch (IOException)
#endif
            {
#if DEBUG
                Logger.GetLogger().Warning("[" + Name + "] Cannot opening " + _dwnldFileName + " error message: " + e.Message);
#endif
            }
        }

        public virtual void HandleExtractEnded()
        {
            ChangeState(State.ExtractEnd);
            if (canRun && firstDownloaded)
                Run();
        }

        internal void HandleRunStart()
        {
            ChangeState(State.RunStart);
        }

        public virtual void HandleRunEnd()
        {
            ChangeState(State.RunEnd);
            OnInstallSuccess();
            ChangeState(State.Done);
        }

        public virtual void Quit(bool doRunOnClose)
        {
            runner.Terminate();
            downloader?.Terminate();
            OnUserQuit();
        }

        public void SetDownloadInfo(ProductSettings settings)
        {
            string downloadFile = ChooseDownloadFileName(settings);
            DwnldLink = string.Empty;
            foreach (ProductSettings.DownloadURL dwnldURL in settings.DownloadURLs)
            {

                if ((dwnldURL.Arch == "32" && OSUtils.Is64BitOperatingSystem()) ||
                    (dwnldURL.Arch == "64" && !OSUtils.Is64BitOperatingSystem()) ||
                    (string.IsNullOrEmpty(dwnldURL.URL)))
                    continue;

                DwnldLink = dwnldURL.URL;
                break;
            }

            string potentialFileName = DwnldLink.Substring(DwnldLink.LastIndexOf('/') + 1);
            if (potentialFileName.Contains("?"))
                potentialFileName = potentialFileName.Substring(0, potentialFileName.IndexOf('?'));
            potentialFileName = string.Join("_", potentialFileName.Split(Path.GetInvalidFileNameChars()));

            if (!potentialFileName.Contains("."))
            {
                var rnd = new Random();
                potentialFileName = rnd.Next(0, int.MaxValue).ToString() + ".tmp";
            }

            if (!Path.HasExtension(downloadFile))
            {
                _dwnldFileName = Path.Combine(downloadFile, potentialFileName);
            }
            else
            {
                _dwnldFileName = downloadFile;
            }
            if (settings.DownloadMethod.ToLower() == PackageDownloaderWebClient.Method)
                downloader = new PackageDownloaderWebClient(this);
            else
                downloader = new PackageDownloaderBits(this);
            _secondDownloadMethod = settings.SecondaryDownloadMethod;
            _dwnldFileName = downloader.UpdateFileNameIfExists(_dwnldFileName);
        }

        public void SetExtractInfo(string compressedFilePath, bool extractable)
        {
            _extractFilePath = compressedFilePath;
            isExtractable = extractable;

            if (!string.IsNullOrEmpty(_extractFilePath) && !ResourcesUtils.IsPathDirectory(compressedFilePath))
            {
                ErrorMessage = $"Extract path not valid: {compressedFilePath}";
                OnInstallFailed(ErrorConsts.ERR_EXTRACT_GENERAL, ErrorMessage);
            }

            if (string.IsNullOrEmpty(_extractFilePath) && Path.GetExtension(_dwnldFileName) == ".zip" && isExtractable)
            {
                _extractFilePath = Path.GetDirectoryName(_dwnldFileName);
            }
        }

        public void SetRunInfo(string fileName, string runParams, int msiTimeout, bool runWithBits)
        {
            RunFileName = fileName;
            if (Path.GetExtension(_dwnldFileName) == ".zip" && isExtractable)
            {
                if (string.IsNullOrEmpty(RunFileName) || ResourcesUtils.IsPathDirectory(RunFileName))
                {
                    ErrorMessage = $"Run path not valid: {RunFileName}";
                    OnInstallFailed(ErrorConsts.ERR_RUN_GENERAL, ErrorMessage);
                }

                RunFileName = Path.Combine(_extractFilePath, RunFileName);
            }
            else
            {
                RunFileName = _dwnldFileName;
            }

            if (msiTimeout == 0)
            {
                msiTimeout = 120000; //2 min default
            }

            this.msiTimeout = msiTimeout;
            RunParams = runParams;
            RunWithBits = runWithBits;
        }

        internal bool HandleFisrtDownloadEnded(object sender, EventArgs e)
        {
            if (((InstallationPackage)sender).InstallationState == State.Error)
            {
                OnUserQuit();
            }
            else
            {
                firstDownloaded = true;
                if (canRun && firstDownloaded && (InstallationState == State.ExtractEnd))
                {
                    Run();
                }
            }
            return firstDownloaded;
        }

        public virtual bool Run()
        {
            if (!File.Exists(RunFileName))
            {
                ErrorMessage = $"No runnable file found in: {RunFileName}, download file name: {_dwnldFileName}, extract file path: {_extractFilePath}";
                OnInstallFailed(ErrorConsts.ERR_RUN_GENERAL, ErrorMessage);
            }
            runner.Run(RunFileName, RunParams);

            return true;
        }

        public void RunDownloadedFile()
        {
            canRun = true;
            if (canRun && firstDownloaded && (InstallationState == State.ExtractEnd))
            {
                Run();
            }
        }


        /**
        * Sets package's status to failed. Sometimes when the packages fails during download or extraction, this method can be 
        * called internally 
        */
        public void HandleInstallFailed(int ErrCode, string ErrMsg = "")
        {
#if DEBUG
            Logger.GetLogger().Error("[" + Name + "](" + ErrCode + ") " + ErrMsg);
#endif
            ChangeState(State.Error);
        }

        internal void SetDownloadProgress(int progressPercentage, long bytesReceived = -1, long totalBytes = -1)
        {
            _dwnldProgress = progressPercentage;
            if (bytesReceived > 0 && bytesReceived >= _dwnldBytesReceived)
            {
                DwnldBytesOffset = bytesReceived - _dwnldBytesReceived;
                _dwnldBytesReceived = bytesReceived;
            }

            DwnldBytesTotal = totalBytes;

            if (_dwnldProgress == 100)
            {
                _dwnldBytesReceived = DwnldBytesTotal;
            }
        }

        public void ChangeState(int newState)
        {
            if (InstallationState >= State.Init && InstallationState != State.Done)
            {
#if DEBUG
                Logger.GetLogger().Info("[" + Name + "] Package switching from InstallationState " + InstallationState + "(" + State.ToString(InstallationState) +
                    ") to InstallationState " + newState + "(" + State.ToString(newState) + ")");
#endif
                InstallationState = newState;
                if (newState < State.Init || newState == State.Done)
                {
                    isProgressCompleted = true;
                }

                OnChangeState?.Invoke(InstallationState);
                HandleProgress?.Invoke(this);
            }
        }

        public static string ChooseDownloadFileName(ProductSettings settings)
        {
            var res = settings.Filename;
            if (!string.IsNullOrEmpty(res) && !Path.HasExtension(res))
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

        internal void HandleDownloadError(string error)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
                ErrorMessage = $"Exception while downloading: {error}";
            else
                ErrorMessage += $", {error}";

            if (String.IsNullOrEmpty(_secondDownloadMethod))
            {
                OnInstallFailed(ErrorConsts.ERR_DOWNLOAD_GENERAL, ErrorMessage);
                HandleProgress(this);
            }
            else
            {
                if (_secondDownloadMethod.ToLower() == PackageDownloaderWebClient.Method)
                    downloader = new PackageDownloaderWebClient(this); 
                else
                    downloader = new PackageDownloaderBits(this);
                _secondDownloadMethod = "";
                downloader.Download(DwnldLink, _dwnldFileName);
            }
        }
    }
}
