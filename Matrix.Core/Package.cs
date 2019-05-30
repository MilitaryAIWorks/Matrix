using System.Collections.Generic;

namespace Matrix.Lib
{
    public class Package
    {

        public string Name { get; }

        public string FileName { get; }

        public string FolderName { get; set; }

        public bool IsInstalled { get; set; }

        public string InstalledVersion { get; set; }    

        public string CurrentVersion { get; set; }

        public int Parts { get; set; }

        public List<string> Updates { get; set; }

        public Package(string name, string fileName, string folderName, bool isInstalled, string version)
        {
            Name = name;
            FileName = fileName;
            FolderName = folderName;
            IsInstalled = isInstalled;
            InstalledVersion = version;
            Updates = new List<string>();
        }

        public Package(string name, string fileName)
        {
            Name = name;
            FileName = fileName;
        }

    }
}
