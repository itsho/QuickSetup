using System;

namespace QuickSetup.Logic.Infra
{
    public class Constants
    {
        public const string APPS_FILE = "AppsList.txt";
        public const string APPLICATIONNAME = "QuickSetup";

        public const string LOREM_IPSUM =
            "Donec ornare consectetur viverra. Duis egestas luctus risus, tincidunt mollis ligula rhoncus a. Nulla lobortis augue urna, ut convallis nulla sagittis in. Aliquam ut nibh sed metus aliquam ultricies malesuada et justo. Vivamus ultrices risus dui, in placerat velit tincidunt posuere. Suspendisse potenti. Nunc vitae tellus pellentesque, pretium neque in, hendrerit tellus. Aliquam hendrerit ante lorem, quis finibus ex sollicitudin eget";

        public static readonly string AppsFileFullPath =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" + APPLICATIONNAME + @"\" +
            APPS_FILE;
    }
}