using System;
using System.Diagnostics;
using QuickSetup.UI.Infra;

namespace QuickSetup.UI.Models
{
    [DebuggerDisplay("Name {AppName}  |  HasReg {ExistenceCheckRegistryKey != string.Empty}  |  HasFile:{ExistenceCheckFilePath != string.Empty}")]
    public class SingleSoftwareModel : IComparable<SingleSoftwareModel>
    {
        public string AppName { get; set; }

        public string NotesToolTip { get; set; }

        public string SetupFileName { get; set; }

        public string SetupSilentParams { get; set; }

        /// <summary>
        /// We cannot run 2 MSI setups at the same time
        /// so, better to avoid that...
        /// </summary>
        public bool IsMsiSetup { get; set; }

        public string ExistenceCheckRegistryKey { get; set; }

        public string ExistenceCheckRegistryValueName { get; set; }
        public string ExistenceCheckRegistryValueData { get; set; }

        /// <summary>
        /// <para>Can use environment variables</para>
        /// <para>for example:</para>
        /// <para>%AppData%</para>
        /// <para>%ProgramData%</para>
        /// <para>%LOCALAPPDATA%</para>
        /// <para>%ProgramFiles%</para>
        /// <para>%ProgramFiles(x86)%</para>
        /// <para>%SystemRoot(x86)%</para>
        /// </summary>
        public string ExistenceCheckFilePath { get; set; }

        public string ExistenceCheckFileMd5Hash { get; set; }

        public string LatestVersionURL { get; set; }

        public int CompareTo(SingleSoftwareModel other)
        {
            Debugger.Break();
            return 0;
        }

        public void CopyDataFrom(SingleSoftwareModel modelSource)
        {
            try
            {
                AppName = modelSource.AppName;
                NotesToolTip = modelSource.NotesToolTip;
                SetupFileName = modelSource.SetupFileName;
                SetupSilentParams = modelSource.SetupSilentParams;
                IsMsiSetup = modelSource.IsMsiSetup;
                ExistenceCheckRegistryKey = modelSource.ExistenceCheckRegistryKey;
                ExistenceCheckRegistryValueName = modelSource.ExistenceCheckRegistryValueName;
                ExistenceCheckRegistryValueData = modelSource.ExistenceCheckRegistryValueData;
                ExistenceCheckFilePath = modelSource.ExistenceCheckFilePath;
                ExistenceCheckFileMd5Hash = modelSource.ExistenceCheckFileMd5Hash;
                LatestVersionURL = modelSource.LatestVersionURL;
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Internal error - Unable to Copy data from SingleSoftwareModel: ", ex);
            }
        }
    }
}