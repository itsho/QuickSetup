using System;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;
using QuickSetup.UI.Models;

namespace QuickSetup.UI.Infra
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

        public static SingleSoftwareModel LoadSingleSoftwareModelFromFile(FileInfo softwareSettingsFile)
        {
            try
            {
                var fileContent = File.ReadAllText(softwareSettingsFile.FullName);
                var model = JsonConvert.DeserializeObject<SingleSoftwareModel>(fileContent);
                return model;
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Internal error - Unable to load single app from file", ex);
                return null;
            }
        }

        public static void SaveSingleSoftwareModelToFile(SingleSoftwareModel model, string fileName)
        {
            try
            {
                var modelAsString = JsonConvert.SerializeObject(model);
                File.WriteAllText(fileName, modelAsString);
                Logger.Log.Info("AppSettings saved to file");
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Internal error - Unable to save single app to file", ex);
            }
        }

        public static AppSettings LoadAppSettingsFromFile()
        {
            try
            {
                if (File.Exists(Constants.AppsFileFullPath))
                {
                    var fileContent = File.ReadAllText(Constants.AppsFileFullPath);
                    var model = JsonConvert.DeserializeObject<AppSettings>(fileContent);
                    
                    // make sure the settings are OK
                    if (!string.IsNullOrEmpty(model.WorkingFolder))
                    {
                        Logger.Log.Info("AppSettings was loaded from file");
                        return model;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Internal error - Unable to load appSettings from file", ex);
            }
            Logger.Log.Info("loading default empty AppSettings");
            return new AppSettings();
        }

        public static void SaveAppSettingsToFile(AppSettings model)
        {
            try
            {
                var modelAsString = JsonConvert.SerializeObject(model);
                File.WriteAllText(Constants.AppsFileFullPath, modelAsString);
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Internal error - Unable to save appSettings to file", ex);
            }
        }
    }
}