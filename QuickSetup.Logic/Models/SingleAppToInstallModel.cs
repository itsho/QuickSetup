using Newtonsoft.Json;
using QuickSetup.Logic.Infra;
using System;
using System.Diagnostics;
using System.IO;

namespace QuickSetup.Logic.Models
{
    [DebuggerDisplay("Name {AppName}  |  HasReg {ExistenceCheckRegistryKey != string.Empty}  |  HasFile:{ExistenceCheckFilePath != string.Empty}")]
    public class SingleSoftwareModel : IComparable<SingleSoftwareModel>
    {
        public string AppName { get; set; }

        /// <summary>
        /// https://www.loc.gov/standards/iso639-2/php/code_list.php
        /// for ex:
        /// eng
        /// fre
        /// </summary>
        public string LangCodeIso6392 { get; set; }

        public string Category { get; set; }

        public string NotesToolTip { get; set; }

        public string SetupFileName { get; set; }

        public string SetupSilentParams { get; set; }

        /// <summary>
        /// We cannot run 2 MSI setups at the same time
        /// so, better to avoid that...
        /// </summary>
        public bool IsMsiSetup { get; set; }

        public string ExistenceCheckRegistryKey { get; set; }

        public string ExistenceCheckRegistryValue { get; set; }

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

        public static SingleSoftwareModel LoadFromFile(FileInfo softwareSettingsFile)
        {
            try
            {
                var fileContent = File.ReadAllText(softwareSettingsFile.FullName);
                var model = JsonConvert.DeserializeObject<SingleSoftwareModel>(fileContent);
                return model;
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Internal error - Unable to load data from softwareSettingsFile ", ex);
                return null;
            }
        }
    }
}