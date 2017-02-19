using QuickSetup.Logic.Infra;
using System;
using System.Diagnostics;

namespace QuickSetup.Logic.Models
{
    [DebuggerDisplay("Name {SoftwareName}  |  HasReg {ExistanceRegistryKey != string.Empty}  |  HasFile:{ExistanceFilePath != string.Empty}")]
    public class SingleSoftwareModel : IComparable<SingleSoftwareModel>
    {
        public string SoftwareName { get; set; }

        /// <summary>
        /// https://www.loc.gov/standards/iso639-2/php/code_list.php
        /// for ex:
        /// eng
        /// fre
        /// </summary>
        public string LangCodeIso6392 { get; set; }

        public string Category { get; set; }

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
        /// <para>Can use environment variables</para>
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

        public int CompareTo(SingleSoftwareModel other)
        {
            Debugger.Break();
            return 0;
        }

        public void CopyDataFrom(SingleSoftwareModel p_modelSource)
        {
            try
            {
                var lstProp = typeof(SingleSoftwareModel).GetProperties();

                foreach (var propertyInfo in lstProp)
                {
                    // get value from 'Source' instance
                    var valOrig = propertyInfo.GetValue(p_modelSource, null);

                    // set value in 'Target' instance (current)
                    propertyInfo.SetValue(this, valOrig);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Internal error - Unable to Copy data from SingleSoftwareModel: ", ex);
            }
        }
    }
}