using Matrix.Lib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Matrix.Lib.Services
{
    public static class PackageService
    {
        public static List<Package> CreateList(Dictionary<string, bool> settings, Dictionary<string, string> versions, string serverPath)
        {
            //Create packages list
            List<Package> packages = new List<Package>
            {
                new Package("MAIW Global Libraries", "GL", "MAIW_GLOBAL", settings["isInstalledGlobalLibraries"], versions["versionGlobalLibraries"]),
                new Package("MAIW Region Africa", "AF", "MAIW_AFRICA", settings["isInstalledRegionAfrica"], versions["versionRegionAfrica"]),
                new Package("MAIW Region Asia", "AS", "MAIW_ASIA", settings["isInstalledRegionAsia"], versions["versionRegionAsia"]),
                new Package("MAIW Region Europe", "EU", "MAIW_EUROPE", settings["isInstalledRegionEurope"], versions["versionRegionEurope"]),
                new Package("MAIW Region North America", "NA", "MAIW_NA", settings["isInstalledRegionNA"], versions["versionRegionNA"]),
                new Package("MAIW Region Oceania", "OC", "MAIW_OCEANIA", settings["isInstalledRegionOceania"], versions["versionRegionOceania"]),
                new Package("MAIW Region South America", "SA", "MAIW_SA", settings["isInstalledRegionSA"], versions["versionRegionSA"])
            };

            //Check each package for updates

            CheckForUpdates(packages, serverPath);

            //Return list
            return packages;
        }

        public static void Uninstall(Package p, string installPath)
        {
            string path = Path.Combine(installPath, p.FolderName);
            if (Directory.Exists(path)) Directory.Delete(path, true);

            string addonFolder = AddonService.GetAddonFolder(p);
            if (Directory.Exists(addonFolder)) Directory.Delete(addonFolder, true);
        }

        //Private methods

        private static void CheckForUpdates(List<Package> packages, string url)
        {
            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load(url + "/packages/versions3.xml");
                XmlNodeList packageNodes = xmlDoc.SelectNodes("//Packages/Package");

                foreach (XmlNode packageNode in packageNodes)
                {
                    //Get package details
                    XmlNode nameNode = packageNode.SelectSingleNode("Name");
                    XmlNode currentNode = packageNode.SelectSingleNode("Current");
                    XmlNodeList versionNodes = packageNode.SelectNodes("Version");

                    string currentVersion = currentNode.InnerText;

                    foreach (Package p in packages)
                    {
                        if (p.Name == nameNode.InnerText)
                        {
                            p.CurrentVersion = currentVersion;

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
