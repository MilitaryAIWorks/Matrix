using System.Collections.Generic;

namespace Matrix.Lib.Models
{
    public class Package
    {
        public string Name { get; }
        public string Tag { get; }
        public string FolderName { get; set; }
        public bool IsInstalled { get; set; }
        public string InstalledVersion { get; set; }    
        public string CurrentVersion { get; set; }
        public List<string> Updates { get; set; }

        public Package(string name, string tag, string folderName, bool isInstalled, string version)
        {
            Name = name;
            Tag = tag;
            FolderName = folderName;
            IsInstalled = isInstalled;
            InstalledVersion = version;
            Updates = new List<string>();
        }
    }
}
