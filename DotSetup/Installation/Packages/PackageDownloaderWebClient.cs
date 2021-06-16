// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using DotSetup.Infrastructure;

namespace DotSetup.Installation.Packages
{
    internal class PackageDownloaderWebClient : PackageDownloader
    {
        public static string Method = "webclient";
        private bool _isDownloading;
        private WebClient _client;
        private string _downloadLink, _outFilePath;
        private readonly Stopwatch _downloadDuration = new Stopwatch();

        public PackageDownloaderWebClient(InstallationPackage installationPackage) : base(installationPackage)
        {
            _isDownloading = false;
        }

        public override bool Download(string downloadLink, string outFilePath)
        {
            bool hasDownloadStarted = true;
            if (!_isDownloading)
            {
                _isDownloading = true;
                _downloadLink = downloadLink;
                _outFilePath = outFilePath;

                try
                {
                    _client = new WebClient();

                    _client.DownloadProgressChanged += HandleDownloadProgress;
                    _client.DownloadFileCompleted += HandleDownloadComplete;
                    _client.DownloadFileAsync(new Uri(downloadLink), _outFilePath);
                    hasDownloadStarted = true;
                    _downloadDuration.Restart();
                }
                catch (Exception ex)
                {
                    HandleWebClientException(ex.Message);
                }
            }
            return hasDownloadStarted;
        }

        private void HandleDownloadProgress(object sender, DownloadProgressChangedEventArgs args)
        {            
            installationPackage.SetDownloadProgress(args.ProgressPercentage, args.BytesReceived, args.TotalBytesToReceive);
            installationPackage.HandleProgress(installationPackage);
        }

        private void HandleDownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            _isDownloading = false;
            if (e.Error != null)
            {
                if (_downloadLink.Contains("https"))
                {
#if DEBUG
                    Logger.GetLogger().Warning("Using https didn't work, trying http. Url: " + _downloadLink);
#endif
                    Download(_downloadLink.Replace("https", "http"), _outFilePath);
                }
                else
                {
                    HandleWebClientException("Failed all attempts to download Url: " + _downloadLink);
                }
            }
            else if (!e.Cancelled)
            {
#if DEBUG
                Logger.GetLogger().Info("Download progress finished for package: " + installationPackage.Name);
#endif

                _downloadDuration.Stop();
                UpdateDownloadTime(_downloadDuration.ElapsedMilliseconds);
                installationPackage.SetDownloadProgress(100);
                installationPackage.RunWithBits = false;
                installationPackage.HandleDownloadEnded();
            }
        }

        private void HandleWebClientException(string ex)
        {
            _isDownloading = false;
            installationPackage.HandleDownloadError(ex);
        }
    }
}
