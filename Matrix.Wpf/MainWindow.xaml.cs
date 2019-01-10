using AutoUpdaterDotNET;
using Ionic.Zip;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Matrix.Lib;
using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinForms = System.Windows.Forms;

namespace Matrix.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        #region MAINWINDOW

        public MainWindow()
        {
            InitializeComponent();
            GetLatestNews();
            GetCredits();
            GetUserSettings();
            packages = packageService.Create(settings, versions, serverPath);
            AutoUpdater.Start($"{serverPath}/app/latestVersion.xml");
        }

        #endregion


        #region FIELDS

        string tempPath = Path.GetTempPath();
        string serverPath = Properties.Settings.Default.ServerPath;
        string installPath;
        bool manual;
        Dictionary<string, bool> settings = new Dictionary<string, bool>();
        Dictionary<string, string> versions = new Dictionary<string, string>();
        PackageService packageService = new PackageService();
        List<Package> packages;
        WebClient client;
        ProgressDialogController progress;
        MessageDialogResult messageResult;
        MessageDialogResult licenseResult;
        string sceneryPath;
        string[] airfields;
        Logger logger = LogManager.GetCurrentClassLogger();

        #endregion


        #region METHODS

        #region Html

        void GetLatestNews()
        {
            string fileName = "latestNews.html";
            string url = $"{serverPath}/html/{fileName}?refreshToken=" + Guid.NewGuid().ToString();

            try
            {
                // Creates an HttpWebRequest.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                // Sends the HttpWebRequest and waits for a response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //Display page if okay.
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    webLatestNews.Source = new Uri(url);
                    webLatestNews.LoadCompleted += (s, e) => webLatestNews.Visibility = Visibility.Visible;
                }

                // Releases the resources of the response.
                response.Close();
                response.Dispose();
            }

            catch (WebException)
            {
                //Something went wrong. Keep the browser hidden.
                webLatestNews.Visibility = Visibility.Hidden;
                txbNoConnection.Visibility = Visibility.Visible;
            }
        }

        void GetCredits()
        {
            string fileName = "credits.html";
            string url = $"{serverPath}/html/{fileName}";

            try
            {
                // Creates an HttpWebRequest.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                // Sends the HttpWebRequest and waits for a response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //Display page if okay.
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    webCredits.Source = new Uri(url);
                    webCredits.LoadCompleted += (s, e) => webCredits.Visibility = Visibility.Visible;
                }

                // Releases the resources of the response.
                response.Close();
                response.Dispose();
            }

            catch (WebException)
            {
                //Something went wrong. Keep the browser hidden.
                webCredits.Visibility = Visibility.Hidden;
                txbNoCredits.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region User Settings

        void GetUserSettings()
        {
            //Check if new version
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }

            //Get installpath and manual setting
            installPath = Properties.Settings.Default.InstallPath;
            manual = Properties.Settings.Default.ManualInstallation;

            //Get the installation settings and store value in public Dictionary settings so they can be used by PackageService
            settings.Add("isInstalledGlobalLibraries", Properties.Settings.Default.IsInstalledGlobalLibraries);
            settings.Add("isInstalledRegionAfrica", Properties.Settings.Default.IsInstalledRegionAfrica);
            settings.Add("isInstalledRegionAsia", Properties.Settings.Default.IsInstalledRegionAsia);
            settings.Add("isInstalledRegionEurope", Properties.Settings.Default.IsInstalledRegionEurope);
            settings.Add("isInstalledRegionNA", Properties.Settings.Default.IsInstalledRegionNA);
            settings.Add("isInstalledRegionOceania", Properties.Settings.Default.IsInstalledRegionOceania);
            settings.Add("isInstalledRegionSA", Properties.Settings.Default.IsInstalledRegionSA);

            //Get the version settings and store value in public Dictionary versions so they can be used by PackageService
            versions.Add("versionGlobalLibraries", Properties.Settings.Default.versionGlobalLibraries);
            versions.Add("versionRegionAfrica", Properties.Settings.Default.versionRegionAfrica);
            versions.Add("versionRegionAsia", Properties.Settings.Default.versionRegionAsia);
            versions.Add("versionRegionEurope", Properties.Settings.Default.versionRegionEurope);
            versions.Add("versionRegionNA", Properties.Settings.Default.versionRegionNA);
            versions.Add("versionRegionOceania", Properties.Settings.Default.versionRegionOceania);
            versions.Add("versionRegionSA", Properties.Settings.Default.versionRegionSA);
        }

        void SaveUserSettings()
        {
            Properties.Settings.Default.IsInstalledGlobalLibraries = packages[0].IsInstalled;
            Properties.Settings.Default.IsInstalledRegionAfrica = packages[2].IsInstalled;
            Properties.Settings.Default.IsInstalledRegionAsia = packages[3].IsInstalled;
            Properties.Settings.Default.IsInstalledRegionEurope = packages[4].IsInstalled;
            Properties.Settings.Default.IsInstalledRegionNA = packages[5].IsInstalled;
            Properties.Settings.Default.IsInstalledRegionOceania = packages[6].IsInstalled;
            Properties.Settings.Default.IsInstalledRegionSA = packages[7].IsInstalled;

            Properties.Settings.Default.versionGlobalLibraries = packages[0].InstalledVersion;
            Properties.Settings.Default.versionRegionAfrica = packages[2].InstalledVersion;
            Properties.Settings.Default.versionRegionAsia = packages[3].InstalledVersion;
            Properties.Settings.Default.versionRegionEurope = packages[4].InstalledVersion;
            Properties.Settings.Default.versionRegionNA = packages[5].InstalledVersion;
            Properties.Settings.Default.versionRegionOceania = packages[6].InstalledVersion;
            Properties.Settings.Default.versionRegionSA = packages[7].InstalledVersion;

            Properties.Settings.Default.InstallPath = installPath;
            Properties.Settings.Default.ManualInstallation = manual;

            Properties.Settings.Default.Save();
        }

        #endregion

        #region Initial States

        void MarkAsInstalled(Button b)
        {
            b.Content = "UNINSTALL";
            b.BorderBrush = Brushes.Green;
        }

        void MarkAsUninstalled(Button b)
        {
            b.Content = "INSTALL";
            b.ClearValue(BorderBrushProperty);
        }

        void MarkAsUpdatesAvailable(Button u, Badged b, Package p)
        {
            u.IsEnabled = true;
            b.Badge = p.Updates.Count;
        }

        void MarkAsNoUpdatesAvailable(Button u, Badged b)
        {
            u.IsEnabled = false;
            b.BadgeBackground = null;
            b.BadgeForeground = null;
        }

        void SetInitialStates()
        {
            //Set install buttons and add regions to cmbRegionPicker

            if (packages[0].IsInstalled) MarkAsInstalled(btnInstallGlobalLibraries);

            if (packages[2].IsInstalled)
            {
                MarkAsInstalled(btnInstallRegionAfrica);
                cmbRegionPicker.Items.Add(packages[2]);
            }

            if (packages[3].IsInstalled)
            {
                MarkAsInstalled(btnInstallRegionAsia);
                cmbRegionPicker.Items.Add(packages[3]);
            }

            if (packages[4].IsInstalled)
            {
                MarkAsInstalled(btnInstallRegionEurope);
                cmbRegionPicker.Items.Add(packages[4]);
            }

            if (packages[5].IsInstalled)
            {
                MarkAsInstalled(btnInstallRegionNA);
                cmbRegionPicker.Items.Add(packages[5]);
            }

            if (packages[6].IsInstalled)
            {
                MarkAsInstalled(btnInstallRegionOceania);
                cmbRegionPicker.Items.Add(packages[6]);
            }

            if (packages[7].IsInstalled)
            {
                MarkAsInstalled(btnInstallRegionSA);
                cmbRegionPicker.Items.Add(packages[7]);
            }

            //Set update buttons
            if (packages[0].IsInstalled && packages[0].Updates.Count != 0) MarkAsUpdatesAvailable(btnUpdateGlobalLibraries, bdgGlobalLibraries, packages[0]); else MarkAsNoUpdatesAvailable(btnUpdateGlobalLibraries, bdgGlobalLibraries);
            if (packages[2].IsInstalled && packages[2].Updates.Count != 0) MarkAsUpdatesAvailable(btnUpdateRegionAfrica, bdgRegionAfrica, packages[2]); else MarkAsNoUpdatesAvailable(btnUpdateRegionAfrica, bdgRegionAfrica);
            if (packages[3].IsInstalled && packages[3].Updates.Count != 0) MarkAsUpdatesAvailable(btnUpdateRegionAsia, bdgRegionAsia, packages[3]); else MarkAsNoUpdatesAvailable(btnUpdateRegionAsia, bdgRegionAsia);
            if (packages[4].IsInstalled && packages[4].Updates.Count != 0) MarkAsUpdatesAvailable(btnUpdateRegionEurope, bdgRegionEurope, packages[4]); else MarkAsNoUpdatesAvailable(btnUpdateRegionEurope, bdgRegionEurope);
            if (packages[5].IsInstalled && packages[5].Updates.Count != 0) MarkAsUpdatesAvailable(btnUpdateRegionNA, bdgRegionNA, packages[5]); else MarkAsNoUpdatesAvailable(btnUpdateRegionNA, bdgRegionNA);
            if (packages[6].IsInstalled && packages[6].Updates.Count != 0) MarkAsUpdatesAvailable(btnUpdateRegionOceania, bdgRegionOceania, packages[6]); else MarkAsNoUpdatesAvailable(btnUpdateRegionOceania, bdgRegionOceania);
            if (packages[7].IsInstalled && packages[7].Updates.Count != 0) MarkAsUpdatesAvailable(btnUpdateRegionSA, bdgRegionSA, packages[7]); else MarkAsNoUpdatesAvailable(btnUpdateRegionSA, bdgRegionSA);

            //Set toggle buttons
            if (manual) tglManual.IsChecked = true;

            //Set install path
            txtInstallationFolder.Text = installPath;
        }

        #endregion

        #region Messages

        private async Task ShowMessageMissingLibs()
        {
            messageResult = await this.ShowMessageAsync("Global Libraries", "The MAIW Global Libraries are required for this region. They will be installed first.", MessageDialogStyle.AffirmativeAndNegative);
        }

        private async Task ShowMessageLicense(string packageName)
        {
            licenseResult = await this.ShowMessageAsync("License", $"Installing the {packageName} package implies that you accept the MAIW license, available at https://militaryaiworks.com/license.", MessageDialogStyle.AffirmativeAndNegative);
        }

        #endregion

        #region Installation Path

        private async Task ChangeInstallPath()
        {
            await this.ShowMessageAsync("Settings", "Please select where you want Matrix to install your military AI traffic.");

            WinForms.FolderBrowserDialog dlg = new WinForms.FolderBrowserDialog();
            dlg.Description = "Please select the folder where you want Matrix to install your military AI traffic.";

            WinForms.DialogResult result = dlg.ShowDialog();
            if (result == WinForms.DialogResult.OK)
            {
                if (Directory.Exists(dlg.SelectedPath))
                {
                    installPath = Path.Combine(dlg.SelectedPath, "Military AI Works\\");
                    txtInstallationFolder.Text = installPath;
                }
                else
                {
                    await this.ShowMessageAsync("Error", "There was a problem with the selected folder, or access to the folder was denied.");
                }
            }
        }

        #endregion

        #region Download

        private async Task DownloadVoicepack(string fileName, string location)
        {
            string url = $"{serverPath}/packages/{fileName}.zip";

            client = new WebClient();
            client.DownloadProgressChanged += DownloadProgressChanged;
            await client.DownloadFileTaskAsync(new Uri(url), location);
        }

        private async Task DownloadFile(string fileName, string location)
        {
            string url = $"{serverPath}/packages/install/{fileName}.zip";
            string filePath = Path.Combine(location, fileName) + ".zip";

            client = new WebClient();
            client.DownloadProgressChanged += DownloadProgressChanged;
            await client.DownloadFileTaskAsync(new Uri(url), filePath);
        }

        private async Task DownloadUpdate(string fileName, string location)
        {
            string url = $"{serverPath}/packages/update/{fileName}.zip";
            string filePath = Path.Combine(location, fileName) + ".zip";

            client = new WebClient();
            client.DownloadProgressChanged += DownloadProgressChanged;
            await client.DownloadFileTaskAsync(new Uri(url), filePath);
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            string received = ((double)e.BytesReceived / 1048576).ToString("0.0");
            string total = ((double)e.TotalBytesToReceive / 1048576).ToString("0.0");

            if (progress.IsCanceled)
            {
                client.CancelAsync();
            }
            else
            {
                progress.SetProgress((double)e.ProgressPercentage / 100);
                progress.SetMessage($"{received} MB of {total} MB");
            }
        }

        #endregion

        #region Unzip

        private async Task UnzipFile(string fileName)
        {
            string source = Path.Combine(tempPath, fileName) + ".zip";

            await Task.Run(() =>
            {
                using (ZipFile zip = new ZipFile(source))
                {
                    zip.ExtractProgress += new EventHandler<ExtractProgressEventArgs>(UnzipProgressChanged);
                    zip.ExtractAll(installPath, ExtractExistingFileAction.OverwriteSilently);
                }
            });

            File.Delete(source);

        }

        private void UnzipProgressChanged(object sender, ExtractProgressEventArgs e)
        {
            double unpacked = e.EntriesExtracted;
            double total = e.EntriesTotal;

            if (e.EntriesTotal != 0)
            {
                progress.SetProgress(unpacked / total);
                progress.SetMessage(e.EntriesExtracted.ToString() + " of " + e.EntriesTotal.ToString() + " files");
            }
        }

        #endregion

        #region Button Clicks

        private async Task ClickInstallButton(Package p, Button b)
        {
            if (p.IsInstalled == false) //Package is not yet installed
            {
                //check install path
                if (string.IsNullOrEmpty(installPath)) await ChangeInstallPath();
                if (string.IsNullOrEmpty(installPath)) return;

                //Show async license message
                await ShowMessageLicense(p.Name);
                if (licenseResult == MessageDialogResult.Affirmative)
                {
                    //Show async progress message
                    progress = await this.ShowProgressAsync("Downloading...", "", true);
                    progress.SetIndeterminate();

                    if (!progress.IsCanceled)
                    {
                        try
                        {
                            //Create filename
                            string filename = p.FileName + "_install_" + p.CurrentVersion;

                            if (p == packages[5])
                            {
                                //Download Part 1
                                progress.SetTitle("Downloading Part 1...");
                                await DownloadFile(filename + "_part1", tempPath);
                                progress.SetMessage("Done!");

                                //Short pause
                                await Task.Delay(1500);

                                //Unzip Part 1
                                progress.SetTitle("Unpacking Part 1...");
                                progress.SetMessage("");
                                progress.SetIndeterminate();
                                progress.SetCancelable(false);
                                await UnzipFile(filename + "_part1");
                                progress.SetMessage("Done!");

                                //Short pause
                                await Task.Delay(1500);

                                //Download Part 2
                                progress.SetTitle("Downloading Part 2...");
                                progress.SetMessage("");
                                progress.SetIndeterminate();
                                await DownloadFile(filename + "_part2", tempPath);
                                progress.SetMessage("Done!");

                                //Short pause
                                await Task.Delay(1500);

                                //Unzip Part 2
                                progress.SetTitle("Unpacking Part 2...");
                                progress.SetMessage("");
                                progress.SetIndeterminate();
                                await UnzipFile(filename + "_part2");
                                progress.SetMessage("Done!");
                            }
                            else
                            {
                                //Download Package
                                await DownloadFile(filename, tempPath);
                                progress.SetMessage("Done!");

                                //Short pause
                                await Task.Delay(1500);

                                //Unzip Package
                                progress.SetTitle("Unpacking...");
                                progress.SetMessage("");
                                progress.SetIndeterminate();
                                progress.SetCancelable(false);
                                await UnzipFile(filename);
                                progress.SetMessage("Done!");
                            }

                            //Short pause
                            await Task.Delay(1500);

                            //Create addon.xml file
                            progress.SetTitle("Creating add-on.xml...");
                            progress.SetMessage("");
                            progress.SetProgress(0);
                            packageService.CreateAddon(p, installPath, manual);
                            await Task.Delay(1500);
                            progress.SetMessage("Done!");
                            progress.SetProgress(1);

                            //Short pause
                            await Task.Delay(1500);

                        }
                        catch (WebException we)
                        {
                            logger.Error(we, "Download Error");
                            await progress.CloseAsync();
                            await this.ShowMessageAsync("Error", $"{p.Name} can not be downloaded.\nPlease check your internet connection and try again.");
                            return;
                        }
                        catch (Exception ex)
                        {
                            if (progress.IsCanceled)
                            {
                                //Close progress without message
                                await progress.CloseAsync();
                            }
                            else
                            {
                                logger.Error(ex, "Install Error");
                                await progress.CloseAsync();
                                await this.ShowMessageAsync("Error", $"Something went wrong while installing {p.Name}.\nPlease try again or contact us on our forums.");
                            }
                            return;
                        }
                    }

                    //Change user setting and button
                    p.IsInstalled = true;
                    p.InstalledVersion = p.CurrentVersion;
                    MarkAsInstalled(b);

                    //Add package to cmbRegionPicker
                    cmbRegionPicker.SelectedItem = null;
                    lstAirfieldPicker.Items.Clear();
                    if (p != packages[0] && !cmbRegionPicker.Items.Contains(p))
                    {
                        cmbRegionPicker.Items.Add(p);
                    }

                    //Close progress and show success message
                    await progress.CloseAsync();
                    await this.ShowMessageAsync("Success!", $"{p.Name} is installed.");
                }
            }

            else //package is installed
            {
                //Clear Airfield Status selection
                cmbRegionPicker.SelectedItem = null;
                lstAirfieldPicker.Items.Clear();

                //Uninstall
                try
                {
                    packageService.Uninstall(p, installPath);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Uninstall Error");
                    await this.ShowMessageAsync("Error", $"Something went wrong while uninstalling {p.Name}.\nPlease try again or contact us on our forums.");
                    return;
                }

                //Change user setting and button
                p.IsInstalled = false;
                MarkAsUninstalled(b);

                //Remove package from cmbRegionPicker
                if (cmbRegionPicker.Items.Contains(p)) cmbRegionPicker.Items.Remove(p);

                //Show success message
                await this.ShowMessageAsync("Success!", $"{p.Name} is uninstalled.");
            }
        }

        private async Task ClickUpdateButton(Package p, Button u, Badged b)
        {
            foreach (string update in p.Updates)
            {
                //Show async progress message
                progress = await this.ShowProgressAsync("Preparing update to version " + update + "...", "", true);
                progress.SetIndeterminate();

                if (!progress.IsCanceled)
                {
                    string fileName = p.FileName + "_update_" + update;

                    try
                    {
                        //Download File Removal List
                        WebRequest request = WebRequest.Create($"{serverPath}/packages/update/{fileName}.txt");
                        WebResponse response = await request.GetResponseAsync();
                        List<string> files = new List<string>();

                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            while (!reader.EndOfStream)
                            {
                                files.Add(reader.ReadLine());
                            }
                        }

                        //Short pause
                        await Task.Delay(1500);

                        //Remove Files
                        progress.SetTitle("Removing files...");
                        progress.SetMessage("");
                        if (files.Count > 0)
                        {
                            foreach (string f in files)
                            {
                                string path = installPath + p.FolderName + f;
                                if (File.Exists(path)) File.Delete(path);
                                else if (Directory.Exists(path)) Directory.Delete(path, true);
                            }
                        }
                        progress.SetMessage("Done!");
                        progress.SetProgress(1);

                        //Short pause
                        await Task.Delay(1500);

                        //Download Update
                        progress.SetTitle("Downloading update...");
                        progress.SetMessage("");
                        progress.SetIndeterminate();
                        await DownloadUpdate(fileName, tempPath);
                        progress.SetMessage("Done!");

                        //Short pause
                        await Task.Delay(1500);

                        //Unzip Update
                        progress.SetTitle("Unpacking update...");
                        progress.SetMessage("");
                        progress.SetIndeterminate();
                        progress.SetCancelable(false);
                        await UnzipFile(fileName);
                        progress.SetMessage("Done!");

                        //Short pause
                        await Task.Delay(1500);

                        //Close progress
                        await progress.CloseAsync();

                    }
                    catch (WebException we)
                    {
                        logger.Error(we, "Download Error");
                        await progress.CloseAsync();
                        await this.ShowMessageAsync("Error", $"The update can not be downloaded.\nPlease check your internet connection and try again.");
                        return;
                    }
                    catch (Exception ex)
                    {
                        if (progress.IsCanceled)
                        {
                            //Close progress without message
                            await progress.CloseAsync();
                        }
                        else
                        {
                            logger.Error(ex, "Update Error");
                            await progress.CloseAsync();
                            await this.ShowMessageAsync("Error", $"Something went wrong while updating {p.Name}.\nPlease try again or contact us on our forums.");
                        }
                        return;
                    }
                }
            }

            //Change user setting and button
            p.InstalledVersion = p.CurrentVersion;
            MarkAsNoUpdatesAvailable(u, b);


            //Show success message
            await this.ShowMessageAsync("Success!", $"{p.Name} is updated.");
        }

        #endregion

        #region Airfield Status

        private void GenerateAirfieldList()
        {
            lstAirfieldPicker.Items.Clear();

            airfields = Directory.GetFiles(sceneryPath);

            foreach (string airfield in airfields)
            {
                string icaoCode = (Path.GetFileName(airfield)).Substring(0, 4);

                if (Path.GetExtension(airfield) == ".off") icaoCode += " (Disabled)";

                if (!lstAirfieldPicker.Items.Contains(icaoCode)) lstAirfieldPicker.Items.Add(icaoCode);
            }
        }

        #endregion

        #endregion


        #region EVENTS

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetInitialStates();
        }

        private void lstMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Link menu to tabs
            if (tclMainWindow != null)
            {
                if (lstMenu.SelectedItem != null)
                {
                    int s = lstMenu.SelectedIndex;
                    tclMainWindow.SelectedIndex = s;
                }
                else lstMenu.SelectedIndex = 0;
            }
        }

        private void btnUserManual_Click(object sender, RoutedEventArgs e)
        {
            //Open user manual PDF
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Matrix-Manual.pdf");
            if (File.Exists(path)) Process.Start(path);
        }

        private void btnSupport_Click(object sender, RoutedEventArgs e)
        {
            //Open support forum in a browser window.
            string url = @"https://militaryaiworks.com/forums/83";
            Process.Start(url);
        }

        private void btnDonate_Click(object sender, RoutedEventArgs e)
        {
            //Open our PayPal donation page in a browser window.
            string url = Properties.Settings.Default.PayPalLink;
            Process.Start(url);
        }

        private async void btnDownloadGlobalVoicepack_Click(object sender, RoutedEventArgs e)
        {

            string fileName = packages[1].FileName;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Save the Global MAIW Voicepack";
            dlg.FileName = fileName;
            dlg.Filter = "Zip file (*.zip)|*.zip";


            if (dlg.ShowDialog() == true)
            {
                progress = await this.ShowProgressAsync("Downloading...", "", true);
                progress.SetIndeterminate();

                if (!progress.IsCanceled)
                {
                    try
                    {
                        await DownloadVoicepack(fileName, dlg.FileName);
                    }
                    catch (WebException we)
                    {
                        logger.Error(we, "Download Error");
                        await progress.CloseAsync();
                        await this.ShowMessageAsync("Error", $"The Global MAIW Voicepack can not be downloaded.\nPlease check your internet connection and try again.");
                        return;
                    }
                    catch (Exception ex)
                    {
                        if (progress.IsCanceled)
                        {
                            await progress.CloseAsync();
                        }
                        else
                        {
                            logger.Error(ex, "Install Error");
                            await progress.CloseAsync();
                            await this.ShowMessageAsync("Error", "Something went wrong while installing the Global MAIW Voicepack.\nPlease try again or contact us on our forums.");
                        }
                        return;
                    }
                }

                await progress.CloseAsync();
                await this.ShowMessageAsync("Success!", "The Global MAIW Voicepack was downloaded succesfully.\nYou will need to unzip the files and import them into EVP manually.");
            }
        }

        private async void btnInstallGlobalLibraries_Click(object sender, RoutedEventArgs e)
        {
            //Check if any regions are installed before uninstalling the global package
            bool regionsInstalled = false;
            for (int i = 2; i < packages.Count; i++)
            {
                if (packages[i].IsInstalled) regionsInstalled = true;
            }

            if (regionsInstalled)
            {
                await this.ShowMessageAsync("Error", "You still have regions installed that depend on the Global MAIW Libraries.");
                return;
            }
            else await ClickInstallButton(packages[0], (Button)sender);

            MarkAsNoUpdatesAvailable(btnUpdateGlobalLibraries, bdgGlobalLibraries);
        }

        private async void btnUpdateGlobalLibraries_Click(object sender, RoutedEventArgs e)
        {
            await ClickUpdateButton(packages[0], btnUpdateGlobalLibraries, bdgGlobalLibraries);
        }

        private async void btnInstallRegionAfrica_Click(object sender, RoutedEventArgs e)
        {
            if (packages[0].IsInstalled) await ClickInstallButton(packages[2], (Button)sender);
            else
            {
                await ShowMessageMissingLibs();

                if (messageResult == MessageDialogResult.Affirmative)
                {
                    await ClickInstallButton(packages[0], btnInstallGlobalLibraries);
                    await ClickInstallButton(packages[2], (Button)sender);
                }
                else return;
            }

            MarkAsNoUpdatesAvailable(btnUpdateRegionAfrica, bdgRegionAfrica);
        }

        private async void btnUpdateRegionAfrica_Click(object sender, RoutedEventArgs e)
        {
            await ClickUpdateButton(packages[2], btnUpdateRegionAfrica, bdgRegionAfrica);
        }

        private async void btnInstallRegionAsia_Click(object sender, RoutedEventArgs e)
        {
            if (packages[0].IsInstalled) await ClickInstallButton(packages[3], (Button)sender);
            else
            {
                await ShowMessageMissingLibs();

                if (messageResult == MessageDialogResult.Affirmative)
                {
                    await ClickInstallButton(packages[0], btnInstallGlobalLibraries);
                    await ClickInstallButton(packages[3], (Button)sender);
                }
                else return;
            }

            MarkAsNoUpdatesAvailable(btnUpdateRegionAsia, bdgRegionAsia);
        }

        private async void btnUpdateRegionAsia_Click(object sender, RoutedEventArgs e)
        {
            await ClickUpdateButton(packages[3], btnUpdateRegionAsia, bdgRegionAsia);
        }

        private async void btnInstallRegionEurope_Click(object sender, RoutedEventArgs e)
        {
            if (packages[0].IsInstalled) await ClickInstallButton(packages[4], (Button)sender);
            else
            {
                await ShowMessageMissingLibs();

                if (messageResult == MessageDialogResult.Affirmative)
                {
                    await ClickInstallButton(packages[0], btnInstallGlobalLibraries);
                    await ClickInstallButton(packages[4], (Button)sender);
                }
                else return;
            }

            MarkAsNoUpdatesAvailable(btnUpdateRegionEurope, bdgRegionEurope);
        }

        private async void btnUpdateRegionEurope_Click(object sender, RoutedEventArgs e)
        {
            await ClickUpdateButton(packages[4], btnUpdateRegionEurope, bdgRegionEurope);
        }

        private async void btnInstallRegionNA_Click(object sender, RoutedEventArgs e)
        {
            if (packages[0].IsInstalled) await ClickInstallButton(packages[5], (Button)sender);
            else
            {
                await ShowMessageMissingLibs();

                if (messageResult == MessageDialogResult.Affirmative)
                {
                    await ClickInstallButton(packages[0], btnInstallGlobalLibraries);
                    await ClickInstallButton(packages[5], (Button)sender);
                }
                else return;
            }

            MarkAsNoUpdatesAvailable(btnUpdateRegionNA, bdgRegionNA);
        }

        private async void btnUpdateRegionNA_Click(object sender, RoutedEventArgs e)
        {
            await ClickUpdateButton(packages[5], btnUpdateRegionNA, bdgRegionNA);
        }

        private async void btnInstallRegionOceania_Click(object sender, RoutedEventArgs e)
        {
            if (packages[0].IsInstalled) await ClickInstallButton(packages[6], (Button)sender);
            else
            {
                await ShowMessageMissingLibs();

                if (messageResult == MessageDialogResult.Affirmative)
                {
                    await ClickInstallButton(packages[0], btnInstallGlobalLibraries);
                    await ClickInstallButton(packages[6], (Button)sender);
                }
                else return;
            }

            MarkAsNoUpdatesAvailable(btnUpdateRegionOceania, bdgRegionOceania);
        }

        private async void btnUpdateRegionOceania_Click(object sender, RoutedEventArgs e)
        {
            await ClickUpdateButton(packages[6], btnUpdateRegionOceania, bdgRegionOceania);
        }

        private async void btnInstallRegionSA_Click(object sender, RoutedEventArgs e)
        {
            if (packages[0].IsInstalled) await ClickInstallButton(packages[7], (Button)sender);
            else
            {
                await ShowMessageMissingLibs();

                if (messageResult == MessageDialogResult.Affirmative)
                {
                    await ClickInstallButton(packages[0], btnInstallGlobalLibraries);
                    await ClickInstallButton(packages[7], (Button)sender);
                }
                else return;
            }

            MarkAsNoUpdatesAvailable(btnUpdateRegionSA, bdgRegionSA);
        }

        private async void btnUpdateRegionSA_Click(object sender, RoutedEventArgs e)
        {
            await ClickUpdateButton(packages[7], btnUpdateRegionSA, bdgRegionSA);
        }

        private void iconReadmeRegionAfrica_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string url = $"{serverPath}/docs/MAIW-RegionAfrica.pdf";
            Process.Start(url);
        }

        private void iconReadmeRegionAsia_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string url = $"{serverPath}/docs/MAIW-RegionAsia.pdf";
            Process.Start(url);
        }

        private void iconReadmeRegionEurope_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string url = $"{serverPath}/docs/MAIW-RegionEurope.pdf";
            Process.Start(url);
        }

        private void iconReadmeRegionNA_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string url = $"{serverPath}/docs/MAIW-RegionNA.pdf";
            Process.Start(url);
        }

        private void iconReadmeRegionOceania_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string url = $"{serverPath}/docs/MAIW-RegionOceania.pdf";
            Process.Start(url);
        }

        private void iconReadmeRegionSA_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string url = $"{serverPath}/docs/MAIW-RegionSA.pdf";
            Process.Start(url);
        }

        private async void btnChangeInstallPath_Click(object sender, RoutedEventArgs e)
        {
            //Check if any regions are installed
            bool regionsInstalled = false;
            for (int i = 2; i < packages.Count; i++)
            {
                if (packages[i].IsInstalled) regionsInstalled = true;
            }

            if (regionsInstalled || packages[0].IsInstalled)
            {
                await this.ShowMessageAsync("Error", "The installation path is locked.\nPlease uninstall all regions and global libraries first.");
                return;
            }
            else await ChangeInstallPath();
        }

        private void tglManual_Checked(object sender, RoutedEventArgs e)
        {
            manual = true;
        }

        private void tglManual_Unchecked(object sender, RoutedEventArgs e)
        {
            manual = false;
        }

        private async void cmbRegionPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Package selectedRegion = (Package)cmbRegionPicker.SelectedItem;

            if (selectedRegion != null)
            {
                try
                {
                    sceneryPath = Path.Combine(installPath, selectedRegion.FolderName, selectedRegion.FolderName) + "_AIRBASES\\Scenery";

                    GenerateAirfieldList();
                    if (lstAirfieldPicker.Items.Count > 0) lstAirfieldPicker.ScrollIntoView(lstAirfieldPicker.Items[0]);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Airfield Status Settings Error");
                    await this.ShowMessageAsync("Error", "Matrix can not find the airbase scenery folder for the selected region.");
                }
            }

        }

        private void btnAirfieldStatus_Click(object sender, RoutedEventArgs e)
        {
            if (lstAirfieldPicker.SelectedItem != null)
            {
                var selectedIndex = lstAirfieldPicker.SelectedIndex;
                string selectedIcaoCode = (lstAirfieldPicker.SelectedItem.ToString()).Substring(0, 4);

                foreach (string airfield in airfields)
                {
                    if (Path.GetFileName(airfield).Substring(0, 4) == selectedIcaoCode)
                    {
                        if (Path.GetExtension(airfield) == ".bgl") File.Move(airfield, Path.ChangeExtension(airfield, ".off")); else File.Move(airfield, Path.ChangeExtension(airfield, ".bgl"));
                    }
                }

                GenerateAirfieldList();
                lstAirfieldPicker.SelectedIndex = selectedIndex;
            }
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            SaveUserSettings();
        }

        #endregion

    }
}
