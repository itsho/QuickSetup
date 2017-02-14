using QuickSetup.Logic.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace QuickSetup.Logic.Infra
{
    public static class Dal
    {
        public static List<SingleSoftwareModel> LoadAll()
        {
            try
            {
                Logger.Log.Info("Loading all apps from file...");
                if (File.Exists(Constants.AppsFileFullPath))
                {
                    var strFileContent = File.ReadAllText(Constants.AppsFileFullPath);
                    var tempAppsList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SingleSoftwareModel>>(strFileContent);

                    return tempAppsList;
                }
                Logger.Log.Fatal("Unable to find AppsFile To load");
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }
            return null;
        }

        public static void SaveAll(List<SingleSoftwareModel> p_lstToSave)
        {
            try
            {
                Logger.Log.Info("Saving all apps into file...");

                // create folder if not exist already
                Directory.CreateDirectory(Path.GetDirectoryName(Constants.AppsFileFullPath));

                // serialize list
                var strSerialized = Newtonsoft.Json.JsonConvert.SerializeObject(p_lstToSave);

                // save content into file
                File.WriteAllText(Constants.AppsFileFullPath, strSerialized);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }
        }
    }
}