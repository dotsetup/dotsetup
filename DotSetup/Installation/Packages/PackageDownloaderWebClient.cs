using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.ComponentModel;

namespace DotSetup
{
    internal class PackageDownloaderWebClient : PackageDownloader
    {
        private bool isDownloading;
        private WebClient client;

        public PackageDownloaderWebClient(InstallationPackage installationPackage) : base(installationPackage)
        {
            isDownloading = false;
        }

        public override bool Download(string downloadLink, string outFilePath)
        {
            bool hasDownloadStarted = true;
            if (!isDownloading)
            {
                isDownloading = true;
                this.downloadLink = downloadLink;
                this.outFilePath = UpdateFileNameIfExists(outFilePath);

                try
                {
                    client = new WebClient();

                    client.DownloadProgressChanged += HandleDownloadProgress;
                    client.DownloadFileCompleted += HandleDownloadComplete;
                    client.DownloadFileAsync(new Uri(downloadLink), this.outFilePath);
                    hasDownloadStarted = true;
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
            installationPackage.handleProgress(installationPackage);
        }

        private void HandleDownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            isDownloading = false;
            if (e.Error != null)
            {
                if (downloadLink.Contains("https"))
                {
                    Logger.GetLogger().Warning("Using https didn't work, trying http. Url: " + downloadLink);
                    Download(downloadLink.Replace("https", "http"), outFilePath);
                }
                else
                {
                    HandleWebClientException("Failed all attempts to download Url: " + downloadLink);
                    installationPackage.handleProgress(installationPackage);
                }
            }
            else if (!e.Cancelled)
            {
                Logger.GetLogger().Info("Download progress finished for package: " + installationPackage.name);
                ValidateFileName();
                installationPackage.SetDownloadProgress(100);
                installationPackage.HandleDownloadEnded();
            }
        }

        private void HandleWebClientException(string ex)
        {
            isDownloading = false;
            ReportDownloadError(ex);
        }

        private void ValidateFileName()
        {
            // Try to extract the filename from the Content-Disposition header
            if (!String.IsNullOrEmpty(client.ResponseHeaders["Content-Disposition"]))
            {
                string headersFileName = client.ResponseHeaders["Content-Disposition"].Substring(client.ResponseHeaders["Content-Disposition"].IndexOf("filename=") + 9).Replace("\"", "");
                if (!String.IsNullOrEmpty(headersFileName) && headersFileName != Path.GetFileName(outFilePath))
                {
                    string oldDwnldFilename = outFilePath;
                    outFilePath = UpdateFileNameIfExists(Path.Combine(Path.GetDirectoryName(oldDwnldFilename), headersFileName));
                    Logger.GetLogger().Info("Changing download filename according to response headers from " + oldDwnldFilename + " to " + outFilePath);
                    File.Move(oldDwnldFilename, outFilePath);
                }
            }
            if (installationPackage.dwnldFileName != outFilePath)
                installationPackage.dwnldFileName = outFilePath;
        }

    }
}