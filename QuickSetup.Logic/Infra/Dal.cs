using QuickSetup.Logic.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace QuickSetup.Logic.Infra
{
    public static class Dal
    {
        public static List<SingleAppToInstallModel> LoadAll()
        {
            try
            {
                if (File.Exists(Constants.AppsFileFullPath))
                {
                    var strFileContent = File.ReadAllText(Constants.AppsFileFullPath);
                    var tempAppsList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SingleAppToInstallModel>>(strFileContent);

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

        //TODO: REMOVE
        public static List<SingleAppToInstallModel> LoadFromOldDB()
        {
            try
            {
                // open old DB
                //string strDSN = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=OldDb/SoftwareList.mdb;password=ItshoItsho";
                string strConnectionString =
    "Provider='Microsoft.Jet.OLEDB.4.0';Data Source=" + "OldDb/SoftwareList.mdb" +
    ";Jet OLEDB:Database Password=" + "ItshoItsho" +
    ";Mode=Share Exclusive;Persist Security Info=True;";

                // Important part - using mdw file
                strConnectionString += "Jet OLEDB:System Database=" +
                    Environment.GetEnvironmentVariable("APPDATA") +
                    @"\Microsoft\Access\system.mdw";

                string strSQL = "SELECT * FROM T_SOFTWARES";
                // create Objects of ADOConnection and ADOCommand
                OleDbConnection myConn = new OleDbConnection(strConnectionString);
                OleDbDataAdapter myCmd = new OleDbDataAdapter(strSQL, myConn);
                myConn.Open();
                DataSet dtSet = new DataSet();

                // read all rows
                myCmd.Fill(dtSet, "T_SOFTWARES");
                DataTable dTable = dtSet.Tables[0];
                List<SingleAppToInstallModel> lstToReturn = new List<SingleAppToInstallModel>();
                foreach (DataRow dtRow in dTable.Rows)
                {
                    // add new row to local list
                    lstToReturn.Add(new SingleAppToInstallModel()
                    {
                        AppName = dtRow["FULLNAME"].ToString(),
                        SetupFolder = dtRow["FOLDER"].ToString(),
                        SetupFileName = dtRow["FILENAME"].ToString(),
                        SetupSilentParams = dtRow["PARAMS"].ToString(),
                        LangCodeIso6392 = GetLang(dtRow["TYPE"].ToString()),
                        IsMsiSetup = bool.Parse(dtRow["IS_MSI"].ToString()),
                        NotesToolTip = dtRow["TOOLTIP"].ToString() + " " + dtRow["TO_CLIPBOARD"].ToString(),
                        ExistanceRegistryKey = dtRow["INSTALLED_REGPATH"].ToString(),
                        ExistanceRegistryValue = dtRow["INSTALLED_REGVALUE"].ToString(),
                        ExistanceFilePath = dtRow["INSTALLED_FILEPATH"].ToString(),
                    });
                }
                return lstToReturn;
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
                return null;
            }
        }

        //TODO: REMOVE
        private static string GetLang(string p_strLangEnum)
        {
            if (p_strLangEnum == "1")
            {
                return "eng";
            }
            else if (p_strLangEnum == "2")
            {
                return "heb";
            }
            else
            {
                return string.Empty;
            }
        }

        public static void SaveAll(List<SingleAppToInstallModel> p_lstToSave)
        {
            try
            {
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