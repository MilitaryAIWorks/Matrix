using AutoUpdaterDotNET;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Matrix.Lib.Models;
using Matrix.Lib.Services;
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
using System.Windows.Input;
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
            packages = PackageService.CreateList(settings, versions, fileServerPath);
            AutoUpdater.Start($"{fileServerPath}/app/latestVersion.xml");
        }

        #endregion


        #region FIELDS

        string tempPath = Path.GetTempPath();
        string fileServerPath = Properties.Settings.Default.FileServerPath;
        string webServerPath = Properties.Settings.Default.WebServerPath;
        string installPath;
        bool manual;
        Dictionary<string, bool> settings = new Dictionary<string, bool>();
        Dictionary<string, string> versions = new Dictionary<string, string>();
        List<Package> packages;
        MessageDialogResult messageResult;
        MessageDialogResult licenseResult;
        string sceneryPath;
        string[] airfields;
        Logger logger = LogManager.GetCurrentClassLogger();
        int totalRegionUpdates = 0;

        #endregion


        #region EVENTS

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetInitialStates();
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            string tag = ((Button)sender).Tag.ToString();
            int packageNumber = 0;

            if (tag == "GL")
            {
                //Check if any regions are installed before uninstalling the global package
                bool regionsInstalled = false;
                for (int i = 1; i < packages.Count; i++)
                {
                    if (packages[i].IsInstalled) regionsInstalled = true;
                }

                if (regionsInstalled)
                {
                    await this.ShowMessageAsync("Error", "You still have regions installed that depend on the Global MAIW Libraries.");
                    return;
                }
                else await InstallPackage(packages[0], "GL");
            }
            else
            {
                switch (tag)
                {
                    case "AF":
                        packageNumber = 1;
                        break;
                    case "AS":
                        packageNumber = 2;
                        break;
                    case "EU":
                        packageNumber = 3;
                        break;
                    case "NA":
                        packageNumber = 4;
                        break;
                    case "OC":
                        packageNumber = 5;
                        break;
                    case "SA":
                        packageNumber = 6;
                        break;
                    default:
                        break;
                }

                if (packages[0].IsInstalled) await InstallPackage(packages[packageNumber], tag);
                else
                {
                    await ShowMessageMissingLibs();

                    if (messageResult == MessageDialogResult.Affirmative)
                    {
                        await InstallPackage(packages[0], "GL");
                        await InstallPackage(packages[packageNumber], tag);
                    }
                    else return;
                }

            }

            MarkAsNoUpdatesAvailable(tag);
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string tag = ((Button)sender).Tag.ToString();
            int packageNumber = 0;
            switch (tag)
            {
                case "AF":
                    packageNumber = 1;
                    break;
                case "AS":
                    packageNumber = 2;
                    break;
                case "EU":
                    packageNumber = 3;
                    break;
                case "NA":
                    packageNumber = 4;
                    break;
                case "OC":
                    packageNumber = 5;
                    break;
                case "SA":
                    packageNumber = 6;
                    break;
                default:
                    break;
            }

            await UpdatePackage(packages[packageNumber], tag);
        }

        private void IconReadme_MouseUp(object sender, MouseButtonEventArgs e)
        {
            string tag = ((MahApps.Metro.IconPacks.PackIconModern)sender).Tag.ToString();
            string url = $"{fileServerPath}/docs/Matrix-Region{tag}.pdf";
            Process.Start(url);
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
            string fileName = "maiw-vp.zip";

            SaveFileDialog dlg = new SaveFileDialog
            {
                Title = "Save the MAIW Global Voicepack",
                FileName = fileName,
                Filter = "Zip file (*.zip)|*.zip"
            };


            if (dlg.ShowDialog() == true)
            {
                var progress = await this.ShowProgressAsync("Downloading...", "", true);
                progress.SetIndeterminate();

                if (!progress.IsCanceled)
                {
                    try
                    {
                        await DownloadService.DownloadFile(fileServerPath, fileName, dlg.FileName, progress);
                    }
                    catch (WebException we)
                    {
                        logger.Error(we, "Download Error");
                        await progress.CloseAsync();
                        await this.ShowMessageAsync("Error", $"The MAIW Global Voicepack can not be downloaded.\nPlease check your internet connection and try again.");
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
                            await this.ShowMessageAsync("Error", "Something went wrong while installing the MAIW Global Voicepack.\nPlease try again or contact us on our forums.");
                        }
                        return;
                    }
                }

                await progress.CloseAsync();
                await this.ShowMessageAsync("Success!", "The MAIW Global Voicepack was downloaded succesfully.\nYou will need to unzip the files and import them into EVP manually.");
            }
        }

        private async void btnChangeInstallPath_Click(object sender, RoutedEventArgs e)
        {
            //Check if any regions are installed
            bool packagesInstalled = false;
            for (int i = 0; i < packages.Count; i++)
            {
                if (packages[i].IsInstalled) packagesInstalled = true;
            }

            if (packagesInstalled)
            {
                await this.ShowMessageAsync("Error", "The installation path is locked.\nPlease uninstall all regions and/or global libraries first.");
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


        #region PRIVATE METHODS

        #region Html

        private void GetLatestNews()
        {
            string fileName = "latestNews.html";
            string url = $"{webServerPath}/html/{fileName}?refreshToken=" + Guid.NewGuid().ToString();

            try
            {
                // Creates an HttpWebRequest.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Referer = "https://militaryaiworks.com";
                request.UserAgent = "Matrix";

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

        private void GetCredits()
        {
            string fileName = "credits.html";
            string url = $"{webServerPath}/html/{fileName}";

            try
            {
                // Creates an HttpWebRequest.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Referer = "https://militaryaiworks.com";
                request.UserAgent = "Matrix";

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

        private void GetUserSettings()
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

        private void SaveUserSettings()
        {
            Properties.Settings.Default.IsInstalledGlobalLibraries = packages[0].IsInstalled;
            Properties.Settings.Default.IsInstalledRegionAfrica = packages[1].IsInstalled;
            Properties.Settings.Default.IsInstalledRegionAsia = packages[2].IsInstalled;
            Properties.Settings.Default.IsInstalledRegionEurope = packages[3].IsInstalled;
            Properties.Settings.Default.IsInstalledRegionNA = packages[4].IsInstalled;
            Properties.Settings.Default.IsInstalledRegionOceania = packages[5].IsInstalled;
            Properties.Settings.Default.IsInstalledRegionSA = packages[6].IsInstalled;

            Properties.Settings.Default.versionGlobalLibraries = packages[0].InstalledVersion;
            Properties.Settings.Default.versionRegionAfrica = packages[1].InstalledVersion;
            Properties.Settings.Default.versionRegionAsia = packages[2].InstalledVersion;
            Properties.Settings.Default.versionRegionEurope = packages[3].InstalledVersion;
            Properties.Settings.Default.versionRegionNA = packages[4].InstalledVersion;
            Properties.Settings.Default.versionRegionOceania = packages[5].InstalledVersion;
            Properties.Settings.Default.versionRegionSA = packages[6].InstalledVersion;

            Properties.Settings.Default.InstallPath = installPath;
            Properties.Settings.Default.ManualInstallation = manual;

            Properties.Settings.Default.Save();
        }

        #endregion

        #region UI States

        private void MarkAsInstalled(string tag)
        {
            var b = (Button)this.FindName($"btnInstall{tag}");
            b.Content = "UNINSTALL";
            b.BorderBrush = Brushes.Green;
        }

        private void MarkAsUninstalled(string tag)
        {
            var i = (Button)this.FindName($"btnInstall{tag}");
            i.Content = "INSTALL";
            i.ClearValue(BorderBrushProperty);
        }

        private void MarkAsUpdatesAvailable(Package p)
        {
            var u = (Button)this.FindName($"btnUpdate{p.Tag}");
            u.IsEnabled = true;

            var b = (Badged)this.FindName($"bdg{p.Tag}");
            b.Badge = p.Updates.Count;

            if (p == packages[0])
            {
                lbiGlobal.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFC80000");
            }
            else
            {
                lbiRegions.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFC80000");
                totalRegionUpdates += p.Updates.Count;
            }
        }

        private void MarkAsNoUpdatesAvailable(string tag)
        {
            var u = (Button)this.FindName($"btnUpdate{tag}");
            u.IsEnabled = false;

            var b = (Badged)this.FindName($"bdg{tag}");
            b.BadgeBackground = null;
            b.BadgeForeground = null;
        }

        private void SetInitialStates()
        {
            foreach (Package p in packages)
            {
                if (p.IsInstalled)
                {
                    MarkAsInstalled(p.Tag);
                    if (p.Updates.Count != 0) MarkAsUpdatesAvailable(p); else MarkAsNoUpdatesAvailable(p.Tag);
                    if (p != packages[0]) cmbRegionPicker.Items.Add(p);
                }
                else
                {
                    MarkAsNoUpdatesAvailable(p.Tag);
                }
            }

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

        #region Install Path

        private async Task ChangeInstallPath()
        {
            await this.ShowMessageAsync("Settings", "Please select where you want Matrix to install your military AI traffic.");

            using (WinForms.FolderBrowserDialog dlg = new WinForms.FolderBrowserDialog
            {
                Description = "Please select the folder where you want Matrix to install your military AI traffic."
            })
            {
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
        }

        #endregion

        #region Package Installation and Update

        private async Task InstallPackage(Package p, string tag)
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
                    var progress = await this.ShowProgressAsync("Downloading...", "", true);
                    progress.SetIndeterminate();

                    if (!progress.IsCanceled)
                    {
                        try
                        {
                            //Generate fileName
                            string fileName = $"maiw_{p.Tag.ToLower()}_f_{p.CurrentVersion}.7z";
                            string filePath = Path.Combine(tempPath, fileName);

                            //Download Package
                            await DownloadService.DownloadFile(fileServerPath, fileName, filePath, progress);
                            progress.SetMessage("Done!");

                            //Short pause
                            await Task.Delay(1500);

                            //Unzip Package
                            progress.SetTitle("Unpacking...");
                            progress.SetMessage("0% unpacked");
                            progress.SetIndeterminate();
                            progress.SetCancelable(false);
                            await ZipService.UnzipFile(Path.Combine(tempPath, fileName), installPath, progress);
                            progress.SetMessage("Done!");

                            //Short pause
                            await Task.Delay(1500);

                            //Create addon.xml file
                            progress.SetTitle("Creating Add-on...");
                            progress.SetMessage("");
                            progress.SetProgress(0);
                            AddonService.CreateAddon(p, installPath, manual);
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
                    MarkAsInstalled(tag);

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
                    PackageService.Uninstall(p, installPath);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Uninstall Error");
                    await this.ShowMessageAsync("Error", $"Something went wrong while uninstalling {p.Name}.\nPlease try again or contact us on our forums.");
                    return;
                }

                //Change user setting and button
                p.IsInstalled = false;
                MarkAsUninstalled(tag);

                //Remove package from cmbRegionPicker
                if (cmbRegionPicker.Items.Contains(p)) cmbRegionPicker.Items.Remove(p);

                //Show success message
                await this.ShowMessageAsync("Success!", $"{p.Name} is uninstalled.");
            }
        }

        private async Task UpdatePackage(Package p, string tag)
        {
            foreach (string update in p.Updates)
            {
                //Show async progress message
                var progress = await this.ShowProgressAsync("Preparing update to version " + update + "...", "", true);
                progress.SetIndeterminate();

                if (!progress.IsCanceled)
                {
                    string fileName = $"maiw_{p.Tag.ToLower()}_u_{update}.7z";

                    try
                    {
                        //Download File Removal List
                        WebRequest request = WebRequest.Create($"{fileServerPath}/packages/txt/maiw_{p.Tag.ToLower()}_u_{update}.txt");
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
                        await DownloadService.DownloadFile(fileServerPath, fileName, tempPath, progress);
                        progress.SetMessage("Done!");

                        //Short pause
                        await Task.Delay(1500);

                        //Unzip Update
                        progress.SetTitle("Unpacking update...");
                        progress.SetMessage("0% unpacked");
                        progress.SetIndeterminate();
                        progress.SetCancelable(false);
                        await ZipService.UnzipFile(Path.Combine(tempPath, fileName), installPath, progress);
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
            if (p == packages[0])
            {
                lbiGlobal.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF252525");
            }
            else
            {
                totalRegionUpdates -= p.Updates.Count;
                if (totalRegionUpdates == 0) lbiRegions.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF252525");
            }

            MarkAsNoUpdatesAvailable(tag);

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

    }
}
