// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public enum State
        {
            Discard = -4, Skipped = -3, Error = -1, Init, DownloadStart, DownloadEnd, ExtractStart, ExtractEnd, RunStart, RunEnd, Done, Confirmed = 100
        }
        private int _dwnldProgress;
        private string _secondDownloadMethod;
        private State _installationState;
        public State InstallationState
        {
            get => _installationState;
            set
            {
                if (value == _installationState || _installationState < State.Init || _installationState == State.Done)
                {
                    return;
                }

                if (value == State.Confirmed)
                {
                    Confirmed = true;
                }
                else
                {
#if DEBUG
                    Logger.GetLogger().Info($"[{Name}] Package switching from InstallationState {_installationState} to InstallationState {value}");
#endif
                    _installationState = value;
                }

                if (value < State.Init || value == State.Done)
                {
                    isProgressCompleted = true;
                }

                OnChangeState?.Invoke(value);
                HandleProgress?.Invoke(this);
            }
        }
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
        internal Action<State> OnChangeState;
        internal Action OnInstallSuccess, OnUserQuit;
        internal Action<int, string> OnInstallFailed;
        internal Action<InstallationPackage> HandleProgress;
        internal PackageDownloader downloader;
        internal PackageExtractor extractor;
        internal PackageRunner runner;
        internal ManualResetEvent onRunWithBits = new ManualResetEvent(false);
        // non optional packages must be confirmed before activation
        private bool _confirmed;
        internal bool Confirmed
        {
            get => _confirmed;
            private set
            {
                if (value)
                {
                    if (!_confirmed)
                    {
#if DEBUG
                        Logger.GetLogger().Info($"[{Name}] package has been confirmed");
#endif                        
                    }
                    _confirmed = true;
                }
                else if (_confirmed)
                {
#if DEBUG
                    Logger.GetLogger().Warning($"[{Name}] package has already been confirmed, can't reverse this");
#endif  
                }
            }
        }
        private bool downloadAndRun, runOnClose;

        public InstallationPackage(ProductSettings settings)
        {
            Name = settings.Name;
            isOptional = settings.IsOptional;
            Confirmed = !isOptional;
            extractor = new PackageExtractor(this);
            runner = new PackageRunner(this);
            InstallationState = State.Init;
            OnInstallFailed += HandleInstallFailed;

            if (!SetCustomEvents(settings.ProductEvents) && !string.IsNullOrEmpty(settings.Behavior))
            {
                downloadAndRun = settings.Behavior.IndexOf("DownloadAndRun", StringComparison.OrdinalIgnoreCase) >= 0;
                runOnClose = (settings.Behavior.IndexOf("RunOnClose", StringComparison.OrdinalIgnoreCase) >= 0) && !downloadAndRun;
                waitForIt = settings.Behavior.IndexOf("RunAndWait", StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        private bool SetCustomEvents(List<ProductSettings.ProductEvent> productEvents)
        {
            bool isSuccess = false;
            if (productEvents == null)
                return isSuccess;
            foreach (ProductSettings.ProductEvent prodEvent in productEvents)
            {
                if (prodEvent.Name == "runOn")
                {
                    runOnClose = (prodEvent.Trigger == "AppClose");
                    downloadAndRun = !runOnClose;
                    isSuccess = true;
                }
            }

            return isSuccess;
        }

        internal bool Activate()
        {
            if (InstallationState != State.Init)
                return false;

            if (!Confirmed)
            {
#if DEBUG
                Logger.GetLogger().Info($"[{Name}] package has not been confirmed before activation hence will be skipped");
#endif
                InstallationState = State.Skipped;
                return false;
            }

            InstallationState = State.DownloadStart;
            if (string.IsNullOrWhiteSpace(DwnldLink))
            {
                HandleDownloadEnded();
                return true;
            }
#if DEBUG
            Logger.GetLogger().Info("[" + Name + "] start downloading from " + DwnldLink + " to location " + _dwnldFileName);
#endif        
            return downloader.Download(DwnldLink, _dwnldFileName);
        }

        public virtual void HandleDownloadEnded()
        {
            SetDownloadProgress(100);
            InstallationState = State.DownloadEnd;
            InstallationState = State.ExtractStart;

            if (string.IsNullOrWhiteSpace(DwnldLink))
            {
                HandleExtractEnded();
                return;
            }
            
            FileStream fop;

            try
            {                
                using (fop = File.OpenRead(_dwnldFileName))
                {
                    FileHash = CryptUtils.ComputeHash(fop, CryptUtils.Hash.SHA1);

                    if (FileUtils.GetMagicNumbers(_dwnldFileName, 2) == "504b" && isExtractable)  //"504b" = "PK" (zip)    
                        extractor.Extract(_dwnldFileName, _extractFilePath);
                    else
                        HandleExtractEnded();
                }
            }
            catch (IOException e)
            {
                HandleDownloadError(e.Message);
            }
            if (downloadAndRun)
                RunDownloadedFile();            
        }

        public virtual void HandleExtractEnded()
        {
            if (runOnClose || string.IsNullOrWhiteSpace(DwnldLink))
                isProgressCompleted = true;
            InstallationState = State.ExtractEnd;
            if (canRun && firstDownloaded)
                Run();
        }

        internal void HandleRunStart()
        {
            InstallationState = State.RunStart;
        }

        public virtual void HandleRunEnd()
        {
            InstallationState = State.RunEnd;
            OnInstallSuccess();
            InstallationState = State.Done;
        }

        public virtual void Quit(bool doRunOnClose)
        {
            if (runOnClose && doRunOnClose)
                RunDownloadedFile();
            runner.Terminate();
            downloader?.Terminate();
            OnUserQuit();
        }

        public void SetDownloadInfo(ProductSettings settings)
        {            
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

            _dwnldFileName = ChooseDownloadFileName(settings.Filename, DwnldLink);
            
            if (settings.DownloadMethod.ToLower() == PackageDownloaderWebClient.Method)
                downloader = new PackageDownloaderWebClient(this);
            else
                downloader = new PackageDownloaderBits(this);
            _secondDownloadMethod = settings.SecondaryDownloadMethod;
            _dwnldFileName = downloader.UpdateFileNameIfExists(_dwnldFileName);
        }

        public void SetExtractInfo(ProductSettings settings)
        {
            _extractFilePath = settings.ExtractPath;
            isExtractable = settings.IsExtractable;

            if (!string.IsNullOrEmpty(_extractFilePath) && !ResourcesUtils.IsPathDirectory(_extractFilePath))
            {
                ErrorMessage = $"Extract path not valid: {_extractFilePath}";
                OnInstallFailed(ErrorConsts.ERR_EXTRACT_GENERAL, ErrorMessage);
            }

            if (string.IsNullOrEmpty(_extractFilePath) && Path.GetExtension(_dwnldFileName) == ".zip" && isExtractable)
            {
                _extractFilePath = Path.GetDirectoryName(_dwnldFileName);
            }
        }

        public void SetRunInfo(ProductSettings settings)
        {
            RunFileName = settings.RunPath;
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

            msiTimeout = settings.MsiTimeoutMS;

            if (msiTimeout == 0)
            {
                msiTimeout = 120000; //2 min default
            }

            RunParams = settings.RunParams;
            RunWithBits = settings.RunWithBits;
            waitForIt = waitForIt || settings.RunAndWait;
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
            InstallationState = State.Error;
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

        public static string ChooseDownloadFileName(string filename, string url)
        {
            string res = filename;            

            if (string.IsNullOrWhiteSpace(res))
            {
                try
                {
                    Uri uri = new Uri(url);
                    if (uri.IsAbsoluteUri)
                    {
                        string potential = Path.GetFileName(uri.LocalPath);
                        if (potential.Contains("?"))
                            potential = potential.Substring(0, potential.IndexOf('?')); ;
                        if (potential.Contains("."))
                            res = potential;
                    }
                }
                catch (Exception)
                {
                }                
            }

            if (string.IsNullOrWhiteSpace(res))
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                Random random = new Random();
                res = new string(Enumerable.Range(1, 5).Select(_ => chars[random.Next(chars.Length)]).ToArray());
            }

            if (!Path.HasExtension(res))
                res += ".exe";            

            if (!(Path.IsPathRooted(res) && !Path.GetPathRoot(res).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)))
                res = Path.Combine(ConfigParser.GetConfig().workDir, res);

            return res;
        }

        internal void HandleDownloadError(string error)
        {
            if (!string.IsNullOrWhiteSpace(ErrorMessage))
                ErrorMessage = $"Exception while downloading: {error}";
            else
                ErrorMessage += $", {error}";

            if (string.IsNullOrEmpty(_secondDownloadMethod))
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
