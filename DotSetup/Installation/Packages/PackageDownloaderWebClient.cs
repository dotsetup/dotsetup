using System;
using System.Net;
using System.ComponentModel;

namespace DotSetup
{
    internal class PackageDownloaderWebClient : PackageDownloader
    {
        public static string Method = "webclient";
        private bool isDownloading;
        private WebClient client;
        private string downloadLink, outFilePath;

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
                this.outFilePath = outFilePath;

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
            installationPackage.HandleProgress(installationPackage);
        }

        private void HandleDownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            isDownloading = false;
            if (e.Error != null)
            {
                if (downloadLink.Contains("https"))
                {
#if DEBUG
                    Logger.GetLogger().Warning("Using https didn't work, trying http. Url: " + downloadLink);
#endif
                    Download(downloadLink.Replace("https", "http"), outFilePath);
                }
                else
                {
                    HandleWebClientException("Failed all attempts to download Url: " + downloadLink);
                }
            }
            else if (!e.Cancelled)
            {
#if DEBUG
                Logger.GetLogger().Info("Download progress finished for package: " + installationPackage.Name);
#endif
                installationPackage.SetDownloadProgress(100);
				installationPackage.RunWithBits = false;
                installationPackage.HandleDownloadEnded();
            }
        }

        private void HandleWebClientException(string ex)
        {
            isDownloading = false;
            installationPackage.HandleDownloadError(ex);
        }
    }
}
