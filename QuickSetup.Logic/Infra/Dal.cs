using QuickSetup.Logic.Models;
using System;
using System.IO;

namespace QuickSetup.Logic.Infra
{
    public static class Dal
    {
        public static AppsListModel AppsList { get; private set; }

        static Dal()
        {
            AppsList = new AppsListModel();
        }

        public static void LoadAll()
        {
            try
            {
                if (File.Exists(Constants.AppsFileFullPath))
                {
                    var strFileContent = File.ReadAllText(Constants.AppsFileFullPath);
                    AppsList = Newtonsoft.Json.JsonConvert.DeserializeObject<AppsListModel>(strFileContent);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }
        }

        public static void SaveAll()
        {
            try
            {
                // create folder if not exist already
                Directory.CreateDirectory(Path.GetDirectoryName(Constants.AppsFileFullPath));

                // serialize list
                var strSerialized = Newtonsoft.Json.JsonConvert.SerializeObject(AppsList);

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