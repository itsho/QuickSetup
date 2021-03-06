﻿using System;
using System.IO;

namespace QuickSetup.UI.Infra
{
    public class Constants
    {
        public const string APPS_FILE = "QuickSetupAppSettings";
        public const string APPLICATIONNAME = "QuickSetup";
        public const string QUICK_SETUP_SETTINGS_FILE_EXTENSION = "QSJson";
        public static readonly string AppsFileFullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APPLICATIONNAME ,APPS_FILE + ".json");


        public const string MVVM_MESSAGE_SHOW_SINGLESOFTWAREVIEW = "SHOW_SINGLESOFTWAREVIEW";
        public const string MVVM_MESSAGE_SHOW_ABOUTVIEW = "SHOW_ABOUT_VIEW";

        public const string HKLM = "HKEY_LOCAL_MACHINE";
        public const string HKCU = "HKEY_CURRENT_USER";
    }
}