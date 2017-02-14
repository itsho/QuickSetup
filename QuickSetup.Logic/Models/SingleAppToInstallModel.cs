using System;
using System.Xml.Serialization;

namespace QuickSetup.Logic.Models
{
    public class SingleAppToInstallModel : IComparable<SingleAppToInstallModel>
    {
        public string AppName
        {
            get; set;
        }

        /// <summary>
        /// https://www.loc.gov/standards/iso639-2/php/code_list.php
        /// for ex:
        /// eng
        /// fre
        /// </summary>
        public string LangCodeIso6392 { get; set; }

        public string NotesToolTip { get; set; }

        public string SetupFolder { get; set; }

        public string SetupFileName { get; set; }

        public string SetupSilentParams { get; set; }

        /// <summary>
        /// We cannot run 2 MSI setups at the same time
        /// so, better to avoid that...
        /// </summary>
        public bool IsMsiSetup { get; set; }

        public string ExistanceRegistryKey { get; set; }

        public string ExistanceRegistryValue { get; set; }

        /// <summary>
        /// <para>Can use special folders.</para>
        /// <para>for example:</para>
        /// <para>%AppData%</para>
        /// <para>%ProgramData%</para>
        /// <para>%LOCALAPPDATA%</para>
        /// <para>%ProgramFiles%</para>
        /// <para>%ProgramFiles(x86)%</para>
        /// <para>%SystemRoot(x86)%</para>
        /// </summary>
        public string ExistanceFilePath { get; set; }

        public string ExistanceFileMd5Hash { get; set; }

        public int CompareTo(SingleAppToInstallModel other)
        {
            throw new NotImplementedException();
        }
    }
}