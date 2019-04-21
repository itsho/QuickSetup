using Newtonsoft.Json;
using QuickSetup.Logic.Models;
using System;
using System.IO;
using System.Security.Cryptography;

namespace QuickSetup.Logic.Infra
{
    public class FilesHelper
    {
        public static string CalculateMd5(string path)
        {
            string strComputedHash;
            // generate md5 hash and compare
            // source - http://stackoverflow.com/a/10520086/426315
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    strComputedHash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "‌​").ToLower();
                }
            }
            return strComputedHash;
        }

        public static SingleSoftwareModel LoadSoftwareSettingsFromFile(FileInfo softwareSettingsFile)
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

        public static void SaveSoftwareSettingsToFile(SingleSoftwareModel model, string fileName)
        {
            try
            {
                var modelAsString = JsonConvert.SerializeObject(model);
                File.WriteAllText(fileName, modelAsString);
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Internal error - Unable to save data to settings file", ex);
            }
        }
    }
}