using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Matrix.Lib.Services
{
    public static class DownloadService
    {
        private static ProgressDialogController progress;

        public static async Task DownloadFile(string fileServerPath, string fileName, string filePath, ProgressDialogController progressDialogController)
        {
            string url = $"{fileServerPath}/packages/download/{fileName}";
            progress = progressDialogController;

            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += DownloadProgressChanged;
                await client.DownloadFileTaskAsync(new Uri(url), filePath);
            }
        }

        private static void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            string received = ((double)e.BytesReceived / 1048576).ToString("0.0");
            string total = ((double)e.TotalBytesToReceive / 1048576).ToString("0.0");

            progress.SetProgress((double)e.ProgressPercentage / 100);
            progress.SetMessage($"{received} MB of {total} MB");
        }

    }
}
