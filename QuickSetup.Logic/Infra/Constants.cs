using System;

namespace QuickSetup.Logic.Infra
{
    public class Constants
    {
        public const string APPS_FILE = "AppsList.txt";
        public const string APPLICATIONNAME = "QuickSetup";

        public static readonly string AppsFileFullPath =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" + APPLICATIONNAME + @"\" +
            APPS_FILE;
    }
}