using System;
using System.IO;
using System.Xml;

namespace Matrix.Lib
{
    public class AddonService
    {
        public void CreateAddon(Package p, string installPath, bool manual)
        {
            string addonFolder = GetAddonFolder(p);

            Directory.CreateDirectory(addonFolder);

            WriteAddonXml(p, addonFolder, installPath);

            if (manual)
            {
                string xmlFile = addonFolder + "\\add-on.xml";
                File.Move(xmlFile, Path.ChangeExtension(xmlFile, ".off"));
            }
        }

        public string GetAddonFolder(Package p)
        {
            string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string addonFolder = myDocs + "\\Prepar3D v4 Add-ons\\" + p.Name;
            return addonFolder;
        }

        //Write Xml

        private readonly XmlWriterSettings settings = new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = true, //Omits the XML declaration since it's not needed in add-on.xml files
            ConformanceLevel = ConformanceLevel.Fragment //Required to make the OmitXmlDeclaration work
        };

        private void WriteAddonXml(Package p, string addonFolder, string installPath)
        {
            string addonPath = $"{addonFolder}\\add-on.xml";
            string subInstallPath = $"{installPath}{p.FolderName}\\{p.FolderName}_";

            using (XmlWriter writer = XmlWriter.Create(addonPath, settings))
            {
                writer.WriteStartElement("SimBase.Document"); //Start SimBase.Document

                writer.WriteAttributeString("Type", "AddOnXml");
                writer.WriteAttributeString("version", "4,0");
                writer.WriteAttributeString("id", "add-on");

                writer.WriteStartElement("AddOn.Name");
                writer.WriteString("Military AI Works" + p.Name.Substring(4));
                writer.WriteEndElement();

                if (p.Name == "MAIW Global Libraries")
                {
                    writer.WriteStartElement("AddOn.Description");
                    writer.WriteString(p.Name.Substring(5) + " required by MAIW regions");
                    writer.WriteEndElement();

                    //Addon.Component Elements

                    writer.WriteStartElement("Addon.Component");
                    writer.WriteElementString("Category", "Effects");
                    writer.WriteElementString("Path", subInstallPath + "EFFECTS");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Addon.Component");
                    writer.WriteElementString("Category", "Scenery");
                    writer.WriteElementString("Path", subInstallPath + "OBJECTS_CUSTOM");
                    writer.WriteElementString("Name", p.Name + " Custom Objects");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Addon.Component");
                    writer.WriteElementString("Category", "Scenery");
                    writer.WriteElementString("Path", subInstallPath + "OBJECTS_GENERIC");
                    writer.WriteElementString("Name", p.Name + " Generic Objects");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Addon.Component");
                    writer.WriteElementString("Category", "Scenery");
                    writer.WriteElementString("Path", subInstallPath + "WAYPOINTS");
                    writer.WriteElementString("Name", p.Name + " Waypoints");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Addon.Component");
                    writer.WriteElementString("Category", "Scripts");
                    writer.WriteElementString("Path", subInstallPath + "SCRIPTS");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Addon.Component");
                    writer.WriteElementString("Category", "Texture");
                    writer.WriteElementString("Path", subInstallPath + "TEXTURES");
                    writer.WriteElementString("Type", "GLOBAL");
                    writer.WriteEndElement();
                }
                else //is region
                {
                    writer.WriteStartElement("AddOn.Description");
                    writer.WriteString("Military AI traffic for" + p.Name.Substring(11) + " by MAIW");
                    writer.WriteEndElement();

                    //Addon.Component Elements

                    writer.WriteStartElement("Addon.Component");
                    writer.WriteElementString("Category", "SimObjects");
                    writer.WriteElementString("Path", subInstallPath + "AIRCRAFT");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Addon.Component");
                    writer.WriteElementString("Category", "Scenery");
                    writer.WriteElementString("Path", subInstallPath + "OBJECTS");
                    writer.WriteElementString("Name", p.Name + " Objects");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Addon.Component");
                    writer.WriteElementString("Category", "Scenery");
                    writer.WriteElementString("Path", subInstallPath + "AIRBASES");
                    writer.WriteElementString("Name", p.Name + " Airbases");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Addon.Component");
                    writer.WriteElementString("Category", "Scenery");
                    writer.WriteElementString("Path", subInstallPath + "WORLD");
                    writer.WriteElementString("Name", p.Name + " World");
                    writer.WriteElementString("Layer", "3");
                    writer.WriteEndElement();
                }
                
                writer.WriteEndElement(); //End SimBase.Document
            }
        }
    }

}
