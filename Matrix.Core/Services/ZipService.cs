using MahApps.Metro.Controls.Dialogs;
using SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Matrix.Lib.Services
{
    public static class ZipService
    {
        private static ProgressDialogController progress;
        public static async Task UnzipFile(string source, string destination, ProgressDialogController progressDialogController)
        {
            progress = progressDialogController;

            await Task.Run(() =>
            {
                // Toggle between the x86 and x64 bit dll
                var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Environment.Is64BitProcess ? "x64" : "x86", "7z.dll");
                SevenZipBase.SetLibraryPath(path);

                // Extract file
                using (SevenZipExtractor extractor = new SevenZipExtractor(source))
                {
                    extractor.Extracting += (s, e) =>
                    {
                        progress.SetProgress(e.PercentDone/100);
                        progress.SetMessage(e.PercentDone + "% unpacked");
                    };
                    extractor.ExtractArchive(destination);
                }
            });
        }
    }
}
