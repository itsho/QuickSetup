using QuickSetup.Logic.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace QuickSetup.Logic.Infra
{
    public static class Dal
    {
        private static readonly Dictionary<string, string> m_dictLang = new Dictionary<string, string>();

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

        public static Dictionary<string, string> LoadListOfLanguagesIso6392()
        {
            try
            {
                if (m_dictLang.Count == 0)
                {
                    using (var fs = File.OpenRead(@"Data\ISO639.csv"))
                    {
                        using (var reader = new StreamReader(fs))
                        {
                            while (!reader.EndOfStream)
                            {
                                var line = reader.ReadLine();
                                if (line != null)
                                {
                                    var values = line.Split(',');

                                    m_dictLang.Add(values[0], values[1]);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while reading list of languages", ex);
            }
            return m_dictLang;
        }
    }
}