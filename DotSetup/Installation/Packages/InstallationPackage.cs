// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DotSetup.Infrastructure;
using DotSetup.Installation.Configuration;
using Microsoft.Win32;

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
namespace DotSetup.Installation.Packages
{
    public partial class InstallationPackage
    {
        public enum State
        {
            Discard = -4,
            Skipped = -3,
            Error = -1,
            Init,
            CheckStart,
            CheckPassed,
            Displayed,
            DownloadStart,
            DownloadEnd,
            ExtractStart,
            ExtractEnd,
            RunStart,
            RunEnd,
            Done,
            AppClose,
            ProcessExecute,
            Confirmed = 100
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
        private string _dwnldLink = string.Empty;
        public string DownloadLink
        {
            get => _dwnldLink;
            set
            {
                if (InstallationState >= State.Init && InstallationState < State.DownloadStart)
                    _dwnldLink = value;
                else
                {
#if DEBUG
                    Logger.GetLogger().Warning($"[{Name}] Cannot switch the download URL to {value} after package activation");
#endif
                }
            }
        }
        internal string FileHash { get; private set; }
        internal string RunParams { get; private set; }
        internal string ErrorMessage { get; set; }
        internal bool hasUpdatedTotal, isOptional, isExtractable;
        internal bool RunWithBits { get; set; }
        internal bool canRun, isProgressCompleted, isUpdatedProgressCompleted;
        internal bool WaitForIt { get; private set; }
        internal bool firstDownloaded = true;
        internal int msiTimeout, runErrorCode, runExitCode;
        public Action<State> OnChangeState;
        public Action OnInstallSuccess, OnUserQuit;
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
        private bool _downloadAndRun, _runOnClose;
        public ProductSettings ProdSettings { get; set; }

        public InstallationPackage(ProductSettings settings)
        {
            ProdSettings = settings;
            Name = settings.Name;
            isOptional = settings.IsOptional;
            Confirmed = !isOptional;
            extractor = new PackageExtractor(this);
            runner = new PackageRunner(this);
            InstallationState = State.Init;
            OnInstallFailed += HandleInstallFailed;

            if (!string.IsNullOrEmpty(settings.Behavior))
            {
                _downloadAndRun = settings.Behavior.IndexOf("DownloadAndRun", StringComparison.OrdinalIgnoreCase) >= 0;
                _runOnClose = settings.Behavior.IndexOf("RunOnClose", StringComparison.OrdinalIgnoreCase) >= 0 && !_downloadAndRun;
                WaitForIt = settings.Behavior.IndexOf("RunAndWait", StringComparison.OrdinalIgnoreCase) >= 0;
            }

            for (int i = 0; i < settings.ProductEvents.Count; i++)
            {
                ProductSettings.ProductEvent prodEvent = settings.ProductEvents[i];
                SetEventTrigger(prodEvent.Name, prodEvent.Trigger, settings, i);
            }                    
        }        

        internal bool Activate()
        {
            if (InstallationState < State.Init || InstallationState >= State.DownloadStart)
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
            if (string.IsNullOrWhiteSpace(DownloadLink))
            {
                HandleDownloadEnded();
                return true;
            }
#if DEBUG
            Logger.GetLogger().Info($"[{Name}] start downloading from {DownloadLink} to location {_dwnldFileName}");
#endif        
            return downloader.Download(DownloadLink, _dwnldFileName);
        }

        public virtual void HandleDownloadEnded()
        {
            SetDownloadProgress(100);
            InstallationState = State.DownloadEnd;
            InstallationState = State.ExtractStart;

            if (string.IsNullOrWhiteSpace(DownloadLink))
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
            if (_downloadAndRun)
                RunDownloadedFile();
        }

        public virtual void HandleExtractEnded()
        {
            InstallationState = State.ExtractEnd;

            if (!_downloadAndRun || string.IsNullOrWhiteSpace(DownloadLink))
                isProgressCompleted = true;
            
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
            OnInstallSuccess?.Invoke();
            InstallationState = State.Done;
        }

        public virtual void Quit(bool doRunOnClose)
        {
            OnChangeState?.Invoke(State.AppClose);
            if (_runOnClose && doRunOnClose)
                RunDownloadedFile();
            runner.Terminate();
            downloader?.Terminate();
            OnUserQuit?.Invoke();
        }

        public void SetDownloadInfo(ProductSettings settings)
        {
            ProdSettings = settings;

            if (string.IsNullOrWhiteSpace(DownloadLink))
            {
                foreach (ProductSettings.DownloadURL dwnldURL in settings.DownloadURLs)
                {

                    if (dwnldURL.Arch == "32" && OSUtils.Is64BitOperatingSystem() ||
                        dwnldURL.Arch == "64" && !OSUtils.Is64BitOperatingSystem() ||
                        string.IsNullOrEmpty(dwnldURL.URL))
                        continue;

                    DownloadLink = dwnldURL.URL;
                    break;
                }
            }

            _dwnldFileName = ChooseDownloadFileName(settings.Filename, DownloadLink);

            if (settings.DownloadMethod.ToLower() == PackageDownloaderWebClient.Method)
                downloader = new PackageDownloaderWebClient(this);
            else
                downloader = new PackageDownloaderBits(this);
            _secondDownloadMethod = settings.SecondaryDownloadMethod;
            _dwnldFileName = downloader.UpdateFileNameIfExists(_dwnldFileName);
        }

        public void SetExtractInfo(ProductSettings settings)
        {
            ProdSettings = settings;
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
            ProdSettings = settings;
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
            WaitForIt = WaitForIt || settings.RunAndWait;
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
                if (canRun && firstDownloaded && InstallationState == State.ExtractEnd)
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
            if (canRun && firstDownloaded && InstallationState == State.ExtractEnd)
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
            string res = filename.Trim();

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
            if (string.IsNullOrWhiteSpace(ErrorMessage))
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
                _secondDownloadMethod = string.Empty;
                downloader.Download(DownloadLink, _dwnldFileName);
            }
        }

        protected delegate void EventDelegate(Dictionary<string,string> parameters);      

        private void HttpFireAndForget(Dictionary<string, string> parameters) => CommunicationUtils.HttpFireAndForget(parameters.Values.FirstOrDefault());

        private void HttpPostAndForget(Dictionary<string, string> parameters)
        {
            string url = parameters.ContainsKey("Url") ? parameters["Url"] : parameters.Values.FirstOrDefault();
            string data = parameters.ContainsKey("Data") ? parameters["Data"] : string.Empty;
            CommunicationUtils.HttpPostAndForget(url, data);
        }

        private void SetRunOnCloseToTrue(Dictionary<string, string> parameters)
        {
            _runOnClose = true;
            _downloadAndRun = false;
        }

        private void SetDownloadAndRunToTrue(Dictionary<string, string> parameters)
        {
            _runOnClose = false;
            _downloadAndRun = true;
        }

        private void RunProcess(Dictionary<string, string> parameters)
        {
            string path = parameters.ContainsKey("Path") ? parameters["Path"] : parameters.Values.FirstOrDefault();
            string arguments = parameters.ContainsKey("Arguments") ? parameters["Arguments"] : string.Empty;
            try
            {
                System.Diagnostics.Process.Start(path, arguments);
#if DEBUG
                Logger.GetLogger().Info($"Running process: {path}, with arguments: {arguments}");
#endif
                OnChangeState?.Invoke(State.ProcessExecute);
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Error($"Cannot execute the process: {path} with arguments: {arguments}, error: {e}");
#endif
            }
        }        

        private void WriteRegKey(Dictionary<string, string> parameters)
        {
            string path = parameters.ContainsKey("Path") ? parameters["Path"] : parameters.Values.FirstOrDefault();
            string subKeyName = parameters.ContainsKey("SubKeyName") ? parameters["SubKeyName"] : string.Empty;
            string subKeyValue = parameters.ContainsKey("SubKeyValue") ? parameters["SubKeyValue"] : string.Empty;
            RegistryValueKind valueKind = (parameters.ContainsKey("ValueKind") && Enum.TryParse(parameters["ValueKind"], out RegistryValueKind kind)) ? kind : RegistryValueKind.String;
            RegistryView view = RegistryView.Default;
            if (parameters.ContainsKey("RegView"))
            {
                view = parameters["RegView"] switch
                {
                    "32" => RegistryView.Registry32,
                    "64" => RegistryView.Registry64,
                    _ => RegistryView.Default,
                };
            }

            if (RegistryUtils.Instance.WriteRegKey(path, subKeyName, subKeyValue, valueKind, view))
            {
#if DEBUG
                Logger.GetLogger().Info($"Wrote registry key in {path}, subKeyName: {subKeyName}, subKeyValue: {subKeyValue}, valueKind: {valueKind}");
#endif
            }
        }

        protected virtual EventDelegate AttachEventDelegate(string name, string trigger, Dictionary<string, string> parameters)
        {
            switch (name.ToUpper())
            {
                case "HTTPGETREQUESTON":
                    if (!UriUtils.CheckURLValid(parameters.Values.FirstOrDefault()))
                    {
#if DEBUG
                        Logger.GetLogger().Error($"[{Name}] The value for the event {name} : {parameters.Values.FirstOrDefault()} is not a well formed Uri");
#endif
                        return null;
                    }
                    return HttpFireAndForget;
                case "HTTPPOSTREQUESTON":
                    string url = parameters.ContainsKey("Url") ? parameters["Url"] : parameters.Values.FirstOrDefault();
                    if (!UriUtils.CheckURLValid(url))
                    {
#if DEBUG
                        Logger.GetLogger().Error($"[{Name}] The value for the event {name} : {url} is not a well formed Uri");
#endif
                        return null;
                    }
                    return HttpPostAndForget;
                case "RUNON":
                    if (parameters.Count > 0)
                        return RunProcess;
                    if (trigger == EventTrigger.AppClose)
                        return SetRunOnCloseToTrue;
                    return SetDownloadAndRunToTrue;
                case "WRITEREGKEYON":
                    return WriteRegKey;
                default:
                    return null;
            }
        }

        private void EventParser(State currentState, State triggerState, EventDelegate eventDelegate, ProductSettings settings, int eventIndex)
        {
            if (currentState != triggerState)
                return;

            eventDelegate(ConfigParser.GetConfig().GetEventParameters(settings, eventIndex));
        }

        protected void SetEventTrigger(string name, string trigger, ProductSettings settings, int eventIndex)
        {
            EventDelegate eventDelegate = AttachEventDelegate(name, trigger, ConfigParser.GetConfig().GetEventParameters(settings, eventIndex));
            
            if (eventDelegate == null)
            {
#if DEBUG
                Logger.GetLogger().Error($"[{Name}] Cannot attach action to event {name} with trigger {trigger}");
#endif
                return;
            }

            State? triggerState = trigger switch
            {
                EventTrigger.Init => State.Init,
                EventTrigger.PreInstall => State.Init,
                EventTrigger.CheckStart => State.CheckStart,
                EventTrigger.CheckPassed => State.CheckPassed,
                EventTrigger.Discarded => State.Discard,
                EventTrigger.Displayed => State.Displayed,
                EventTrigger.Confirmed => State.Confirmed,
                EventTrigger.Skipped => State.Skipped,
                EventTrigger.DownloadStart => State.DownloadStart,
                EventTrigger.DownloadEnd => State.DownloadEnd,
                EventTrigger.ExtractStart => State.ExtractStart,
                EventTrigger.ExtractEnd => State.ExtractEnd,
                EventTrigger.RunStart => State.RunStart,
                EventTrigger.RunEnd => State.RunEnd,
                EventTrigger.PostInstall => State.RunEnd,
                EventTrigger.Done => State.Done,
                EventTrigger.AppClose => State.AppClose,
                EventTrigger.ProcessExecute => State.ProcessExecute,
                EventTrigger.Error => State.Error,
                _ => null,
            };

            if (triggerState == null)
            {
#if DEBUG
                Logger.GetLogger().Error($"[{Name}] No event trigger called {trigger} for event type {name}");
#endif
                return;
            }

            if (triggerState == State.Init)
                EventParser(State.Init, State.Init, eventDelegate, settings, eventIndex);
            else
                OnChangeState += (state) => { EventParser(state, (State)triggerState, eventDelegate, settings, eventIndex); }; 
        }
    } 
}
