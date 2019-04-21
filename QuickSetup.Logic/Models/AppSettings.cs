using System.Collections.Generic;

namespace QuickSetup.Logic.Models
{
    public class AppSettings
    {
        public List<string> RecentWorkingFolders { get; set; }
        public string WorkingFolder { get; set; }
        public bool IsShowAllFolders { get; set; }

        public AppSettings()
        {
            RecentWorkingFolders = new List<string>();
        }
    }
}