using System;
using System.Xml.Serialization;

namespace QuickSetup.Logic.Models
{
    [Serializable]
    public class SingleAppToInstallModel : IComparable<SingleAppToInstallModel>
    {
        [XmlAttribute]
        public string AppName
        {
            get; set;
        }

        [XmlAttribute]
        public string Folder
        {
            get; set;
        }

        [XmlAttribute]
        public string NotesToolTip { get; set; }

        [XmlAttribute]
        public string SetupFileName { get; set; }

        [XmlAttribute]
        public string SilentSetupParams { get; set; }

        /// <summary>
        /// it's impossible to run 2 MSI at the same time
        /// so, better notice that...
        /// </summary>
        [XmlAttribute]
        public bool IsMsiSetup { get; set; }

        [XmlAttribute]
        public string ExistanceRegistryKey { get; set; }

        [XmlAttribute]
        public string ExistanceRegistryValue { get; set; }

        [XmlAttribute]
        public string ExistanceFileProgramFiles64 { get; set; }

        [XmlAttribute]
        public string ExistanceFileProgramFiles86 { get; set; }

        public int CompareTo(SingleAppToInstallModel other)
        {
            throw new NotImplementedException();
        }
    }
}