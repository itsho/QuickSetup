using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using QuickSetup.Logic.Infra;
using QuickSetup.Logic.Infra.Enums;
using QuickSetup.Logic.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Input;

namespace QuickSetup.UI.ViewModel
{
    public class SingleSoftwareViewModel : ViewModelBase
    {
        #region consts

        private const string HKLM = "HKEY_LOCAL_MACHINE";
        private const string HKCU = "HKEY_LOCAL_MACHINE";

        #endregion consts

        #region Data members

        private bool _blnIsChecked;
        private SoftwareInstallStatusEnum _status;

        #endregion Data members

        #region CTOR

        public SingleSoftwareViewModel(SingleSoftwareModel p_singleSoftwareModel, List<string> p_lstPossibleCategories)
        {
            if (p_singleSoftwareModel == null)
            {
                Logger.Log.Fatal("Invalid software loaded! software might crash");
                return;
            }

            OriginalModel = p_singleSoftwareModel;
            ClonedModel = new SingleSoftwareModel();
            ClonedModel.CopyDataFrom(p_singleSoftwareModel);

            InstallCommand = new RelayCommand(OnInstallCommand, CanExecuteInstallCommand);
            EditSoftwareCommand = new RelayCommand(OnEditSoftwareCommand, () => true);
            TranslatePathToEnvVarCommand = new RelayCommand(OnTranslatePathToEnvVarCommand, CanExecuteTranslatePathToEnvVarCommand);
            CalculateMD5OfExistanceFilePathCommand = new RelayCommand(OnCalculateMD5OfExistanceFilePathCommand, CanExecuteCalculateMD5OfExistanceFilePathCommand);
            DiscardAndCloseCommand = new RelayCommand(OnDiscardAndCloseCommand);
            SaveChangesAndCloseCommand = new RelayCommand(OnSaveChangesAndCloseCommand);
            BrowseToSelectSetupFileCommand = new RelayCommand(OnBrowseToSelectSetupFileCommand);
            OpenRegistryKeyCommand = new RelayCommand(OnOpenRegistryKeyCommand, CanExecuteOpenRegistryKeyCommand);

            RefreshSoftwareInstallStatusEnum();

            ListOfIso6392 = Dal.LoadListOfLanguagesIso6392();
            PossibleCategories = new List<string>();
            if (p_lstPossibleCategories != null)
            {
                PossibleCategories.AddRange(p_lstPossibleCategories);
            }
        }

        #endregion CTOR

        #region Properties

        public event CloseWindowRequested OnCloseWindowRequested;

        public delegate void CloseWindowRequested(bool p_blnIsSaveRequested);

        public SingleSoftwareModel ClonedModel { get; private set; }

        public SingleSoftwareModel OriginalModel { get; private set; }

        public Dictionary<string, string> ListOfIso6392 { get; private set; }

        public List<string> PossibleCategories { get; private set; }

        public ICommand InstallCommand { get; private set; }

        public ICommand EditSoftwareCommand { get; private set; }

        public ICommand TranslatePathToEnvVarCommand { get; private set; }

        public ICommand CalculateMD5OfExistanceFilePathCommand { get; private set; }

        public ICommand DiscardAndCloseCommand { get; private set; }

        public ICommand SaveChangesAndCloseCommand { get; private set; }

        public ICommand BrowseToSelectSetupFileCommand { get; private set; }

        public ICommand OpenRegistryKeyCommand { get; private set; }

        public SoftwareInstallStatusEnum Status
        {
            get { return _status; }
            set { Set(ref _status, value, nameof(Status)); }
        }

        public bool IsChecked
        {
            get { return _blnIsChecked; }
            set { Set(ref _blnIsChecked, value, nameof(IsChecked)); }
        }

        #endregion Properties

        #region Private methods

        private void OnEditSoftwareCommand()
        {
            try
            {
                Messenger.Default.Send(new NotificationMessage(this, Constants.MVVM_MESSAGE_SHOW_SINGLESOFTWAREVIEW));
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while editing software", ex);
            }
        }

        private void OnInstallCommand()
        {
            try
            {
                var strArgument = @"/select, """ + Path.Combine(ClonedModel.SetupFolder, ClonedModel.SetupFileName) + @"""";
                Process.Start("explorer.exe", strArgument);
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while installing", ex);
            }
        }

        private bool CanExecuteInstallCommand()
        {
            try
            {
                var strFullPath = Path.Combine(ClonedModel.SetupFolder, ClonedModel.SetupFileName);
                return File.Exists(strFullPath);
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while checking if setup file exists", ex);

                // we are not sure if the file exists, yet, I prefer to allow user to click it
                return true;
            }
        }

        private bool CanExecuteTranslatePathToEnvVarCommand()
        {
            return !string.IsNullOrWhiteSpace(ClonedModel.ExistanceFilePath);
        }

        private void OnTranslatePathToEnvVarCommand()
        {
            try
            {
                var lstEnvironmentVariables = Environment.GetEnvironmentVariables();

                Debugger.Break();
                if (!string.IsNullOrWhiteSpace(ClonedModel.ExistanceFilePath))
                {
                    foreach (DictionaryEntry envVariable in lstEnvironmentVariables)
                    {
                        if (envVariable.Key.ToString() == "HOMEDRIVE")
                        {
                            continue;
                        }

                        if (ClonedModel.ExistanceFilePath.StartsWith(envVariable.Value.ToString(), StringComparison.Ordinal))
                        {
                            // replace simple string with environment variable
                            ClonedModel.ExistanceFilePath =
                                ClonedModel.ExistanceFilePath.Replace(envVariable.Value.ToString(),
                                    string.Format("%{0}%", envVariable.Key));
                            RaisePropertyChanged(nameof(ClonedModel));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while Translating path to environment variables", ex);
            }
        }

        private bool CanExecuteCalculateMD5OfExistanceFilePathCommand()
        {
            return !string.IsNullOrWhiteSpace(ClonedModel.ExistanceFilePath);
        }

        private void OnCalculateMD5OfExistanceFilePathCommand()
        {
            try
            {
                var path = Environment.ExpandEnvironmentVariables(ClonedModel.ExistanceFilePath);

                if (File.Exists(path))
                {
                    ClonedModel.ExistanceFileMd5Hash = CalculateMd5(path);
                    RaisePropertyChanged(nameof(ClonedModel));
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while Translating path to environment variables", ex);
            }
        }

        private static string CalculateMd5(string path)
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

        private void OnSaveChangesAndCloseCommand()
        {
            // copy all values to original model
            OriginalModel.CopyDataFrom(ClonedModel);
            RaisePropertyChanged(nameof(OriginalModel));

            // check if software is installed on local machine
            RefreshSoftwareInstallStatusEnum();

            // notify main model that the window is closed
            RaiseCloseWindowRequested(true);
        }

        private void OnDiscardAndCloseCommand()
        {
            RaiseCloseWindowRequested(false);
        }

        private void OnBrowseToSelectSetupFileCommand()
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Setup files (*.exe, *.msi, *.bat, *.cmd, *.vbs) | *.exe; *.msi; *.bat; *.cmd; *.vbs";
                dialog.Title = "Please select setup file.";
                if (dialog.ShowDialog().GetValueOrDefault(false))
                {
                    ClonedModel.SetupFolder = Path.GetDirectoryName(dialog.FileName);
                    ClonedModel.SetupFileName = Path.GetFileName(dialog.FileName);
                    RaisePropertyChanged(nameof(ClonedModel));
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while getting app status", ex);
            }
        }

        private void OnOpenRegistryKeyCommand()
        {
            try
            {
                // I want to avoid RegJump since I'm not sure if the way i'm using it is allowed for distribution

                var lstProcRegEdit = Process.GetProcessesByName("regedit");
                if (lstProcRegEdit.Length > 0)
                {
                    Logger.Log.Info("Registry Editor already open. closing and re-opening.");
                    foreach (var process in lstProcRegEdit)
                    {
                        process.Kill();
                    }
                }

                //http://stackoverflow.com/a/12516008/426315
                var keyRegedit =
                    Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit",
                        RegistryKeyPermissionCheck.ReadWriteSubTree);
                if (keyRegedit != null)
                {
                    var arrRegPath = ClonedModel.ExistanceRegistryKey.Split('\\');

                    // remove reg value to get the key alone
                    var strPathKey = ClonedModel.ExistanceRegistryKey.Replace("\\" + arrRegPath.LastOrDefault(), string.Empty);
                    // set LastKey value
                    keyRegedit.SetValue("LastKey", strPathKey, RegistryValueKind.String);
                }

                // start regedit
                Process.Start("regedit");
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while editing software", ex);
            }
        }

        private bool CanExecuteOpenRegistryKeyCommand()
        {
            return !string.IsNullOrEmpty(ClonedModel.ExistanceRegistryKey);
        }

        #endregion Private methods

        #region public methods

        public void RefreshSoftwareInstallStatusEnum()
        {
            try
            {
                Status = SoftwareInstallStatusEnum.Unknown;

                // check registry value
                if (!string.IsNullOrWhiteSpace(ClonedModel.ExistanceRegistryKey) &&
                    !string.IsNullOrWhiteSpace(ClonedModel.ExistanceRegistryValue))
                {
                    //http://stackoverflow.com/a/22825275/426315
                    var registryRoot = Registry.CurrentUser;
                    var strKeyToOpen = ClonedModel.ExistanceRegistryKey;

                    if (ClonedModel.ExistanceRegistryKey.StartsWith(HKLM, StringComparison.Ordinal))
                    {
                        registryRoot = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                        strKeyToOpen = ClonedModel.ExistanceRegistryKey.Replace(HKLM + @"\", string.Empty);
                    }
                    else if (ClonedModel.ExistanceRegistryKey.StartsWith(HKCU, StringComparison.Ordinal))
                    {
                        registryRoot = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                        strKeyToOpen = ClonedModel.ExistanceRegistryKey.Replace(HKCU + @"\", string.Empty);
                    }

                    var registryKey = registryRoot.OpenSubKey(strKeyToOpen);

                    // if key exists
                    if (registryKey != null)
                    {
                        var strValue = registryKey.GetValue(ClonedModel.ExistanceRegistryValue) as string;
                        if (strValue != null)
                        {
                            Status = SoftwareInstallStatusEnum.Installed;
                        }
                        else
                        {
                            Status = SoftwareInstallStatusEnum.NotInstalled;
                        }
                    }
                    // if key does not exists
                    else
                    {
                        Status = SoftwareInstallStatusEnum.NotInstalled;
                    }
                }

                // check file existance - this will override Registry check
                if (!string.IsNullOrWhiteSpace(ClonedModel.ExistanceFilePath))
                {
                    var path = Environment.ExpandEnvironmentVariables(ClonedModel.ExistanceFilePath);

                    // file exists :-)
                    if (File.Exists(path))
                    {
                        // check hash
                        if (!string.IsNullOrWhiteSpace(ClonedModel.ExistanceFileMd5Hash))
                        {
                            string strComputedHash = CalculateMd5(path);

                            if (strComputedHash == ClonedModel.ExistanceFileMd5Hash)
                            {
                                Status = SoftwareInstallStatusEnum.Installed;
                            }
                        }
                        else
                        {
                            Status = SoftwareInstallStatusEnum.Installed;
                        }
                    }
                    // file not exists
                    else
                    {
                        Status = SoftwareInstallStatusEnum.NotInstalled;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while getting app status", ex);
                Status = SoftwareInstallStatusEnum.UnableToGetStatus;
            }
        }

        protected virtual void RaiseCloseWindowRequested(bool p_blnIsSaveRequested)
        {
            OnCloseWindowRequested?.Invoke(p_blnIsSaveRequested);
        }

        #endregion public methods
    }
}