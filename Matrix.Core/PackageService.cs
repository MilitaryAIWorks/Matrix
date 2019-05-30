using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Matrix.Lib
{
    public class PackageService
    {
        public List<Package> Create(Dictionary<string, bool> settings, Dictionary<string, string> versions, string serverPath)
        {
            //Create packages list
            List<Package> packages = new List<Package>
            {
                new Package("MAIW Global Libraries", "maiwGlobalLibraries", "MAIW_GLOBAL", settings["isInstalledGlobalLibraries"], versions["versionGlobalLibraries"]),
                new Package("MAIW Global Voicepack", "maiwGlobalVoicepack"),
                new Package("MAIW Region Africa", "maiwRegionAfrica", "MAIW_AFRICA", settings["isInstalledRegionAfrica"], versions["versionRegionAfrica"]),
                new Package("MAIW Region Asia", "maiwRegionAsia", "MAIW_ASIA", settings["isInstalledRegionAsia"], versions["versionRegionAsia"]),
                new Package("MAIW Region Europe", "maiwRegionEurope", "MAIW_EUROPE", settings["isInstalledRegionEurope"], versions["versionRegionEurope"]),
                new Package("MAIW Region North America", "maiwRegionNA", "MAIW_NA", settings["isInstalledRegionNA"], versions["versionRegionNA"]),
                new Package("MAIW Region Oceania", "maiwRegionOceania", "MAIW_OCEANIA", settings["isInstalledRegionOceania"], versions["versionRegionOceania"]),
                new Package("MAIW Region South America", "maiwRegionSA", "MAIW_SA", settings["isInstalledRegionSA"], versions["versionRegionSA"])
            };

            //Check each package for updates

            UpdateChecker(packages, serverPath);

            //Return list
            return packages;
        }

        public void CreateAddon(Package p, string installPath, bool manual)
        {
            string addonFolder = GetAddonFolder(p);

            Directory.CreateDirectory(addonFolder);

            AddonService addonService = new AddonService();

            if (p.Name == "MAIW Global Libraries")
            {
                addonService.WriteGlobalAddonXml(p, addonFolder, installPath);
            }
            else
            {
                addonService.WriteRegionAddonXml(p, addonFolder, installPath);
            }

            if (manual)
            {
                string xmlFile = addonFolder + "\\add-on.xml";
                File.Move(xmlFile, Path.ChangeExtension(xmlFile, ".off"));
            }
        }


        public void Uninstall(Package p, string installPath)
        {
            string path = Path.Combine(installPath, p.FolderName);
            if (Directory.Exists(path)) Directory.Delete(path, true);

            string addonFolder = GetAddonFolder(p);
            if (Directory.Exists(addonFolder)) Directory.Delete(addonFolder, true);
        }

        private string GetAddonFolder(Package p)
        {
            string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string addonFolder = myDocs + "\\Prepar3D v4 Add-ons\\" + p.Name;
            return addonFolder;
        }

        private void UpdateChecker(List<Package> packages, string url)
        {
            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load(url + "/packages/versions2.xml");
                XmlNodeList packageNodes = xmlDoc.SelectNodes("//Packages/Package");

                foreach (XmlNode packageNode in packageNodes)
                {
                    //Get package details
                    XmlNode nameNode = packageNode.SelectSingleNode("Name");
                    XmlNode partsNode = packageNode.SelectSingleNode("Parts");
                    XmlNode currentNode = packageNode.SelectSingleNode("Current");
                    XmlNodeList versionNodes = packageNode.SelectNodes("Version");

                    string currentVersion = currentNode.InnerText;
                    int parts = int.Parse(partsNode.InnerText);

                    foreach (Package p in packages)
                    {
                        if (p.Name == nameNode.InnerText)
                        {
                            p.CurrentVersion = currentVersion;
                            p.Parts = parts;

                            if (p.IsInstalled && p.InstalledVersion != p.CurrentVersion)
                            {
                                for (int i = (versionNodes.Count - 1); i >= 0; i--)
                                {
                                    XmlNode versionNode = versionNodes[i];
                                    if (versionNode.InnerText == p.InstalledVersion) break;
                                    p.Updates.Insert(0, versionNode.InnerText);
                                }
                            }

                        }
                    }
                }
            }
            catch { }

        }
    }
}
