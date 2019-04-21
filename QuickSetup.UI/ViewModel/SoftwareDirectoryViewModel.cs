using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using QuickSetup.Logic.Infra;
using QuickSetup.Logic.Infra.Enums;
using QuickSetup.Logic.Models;
using QuickSetup.UI.Infra;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace QuickSetup.UI.ViewModel
{
    public class SoftwareDirectoryViewModel : ViewModelBase
    {
        #region Data members

        private readonly DirectoryInfo _directoryInfo;

        private SoftwareInstallStatusEnum _status;
        private SingleSoftwareModel _originalModel;
        private SingleSoftwareModel _clonedModel;

        #endregion Data members

        #region CTOR

        public SoftwareDirectoryViewModel(string root) : this(new DirectoryInfo(root))
        {
        }

        public SoftwareDirectoryViewModel(DirectoryInfo root)
        {
            _directoryInfo = root;
            SubDirs = new List<SoftwareDirectoryViewModel>();
            InstallSoftwareCommand = new RelayCommand(OnInstallSoftwareCommand, CanExecuteInstallSoftwareCommand);
            EditQSSettingsCommand = new RelayCommand(OnEditQSSettingsCommand);
            BrowseToFolderCommand = new RelayCommand(OnBrowseCommand);

            TranslatePathToEnvVarCommand = new RelayCommand(OnTranslatePathToEnvVarCommand, CanExecuteTranslatePathToEnvVarCommand);
            CalculateMD5OfExistenceFilePathCommand = new RelayCommand(OnCalculateMD5OfExistenceFilePathCommand, CanExecuteCalculateMD5OfExistenceFilePathCommand);
            DiscardAndCloseCommand = new RelayCommand<ICloseable>(OnDiscardAndCloseCommand);
            SaveChangesAndCloseCommand = new RelayCommand<ICloseable>(OnSaveChangesAndCloseCommand);
            BrowseToSelectSetupFileCommand = new RelayCommand(OnBrowseToSelectSetupFileCommand);
            OpenRegistryKeyCommand = new RelayCommand(OnOpenRegistryKeyCommand, CanExecuteOpenRegistryKeyCommand);
            BrowseToExistenceCheckFileCommand = new RelayCommand(OnBrowseToExistenceCheckFileCommand);

            //TODO: Dal.LoadListOfLanguagesIso6392();
            ListOfIso6392 = new List<string>();

            //TODO: load categories;
            PossibleCategories = new List<string>();
        }

        #endregion CTOR

        #region Properties

        public SingleSoftwareModel OriginalModel
        {
            get => _originalModel;
            private set
            {
                if (_originalModel != value)
                {
                    _originalModel = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(CurrentFolder));
                }
            }
        }

        /// <summary>
        /// in use for temporary edits by user before save
        /// </summary>
        public SingleSoftwareModel ClonedModel
        {
            get => _clonedModel;
            private set
            {
                if (_clonedModel != value)
                {
                    _clonedModel = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string CurrentFolder
        {
            get
            {
               
                return _directoryInfo.FullName;
            }
        }

        public List<SoftwareDirectoryViewModel> SubDirs { get; private set; }

        public string Name
        {
            get
            {
                if (OriginalModel != null && !string.IsNullOrEmpty(OriginalModel.AppName))
                {
                    return OriginalModel.AppName;
                }
                return _directoryInfo.Name;
            }
        }

        public List<string> ListOfIso6392 { get; private set; }
        public List<string> PossibleCategories { get; private set; }

        public ICommand InstallSoftwareCommand { get; private set; }
        public ICommand EditQSSettingsCommand { get; private set; }
        public ICommand BrowseToFolderCommand { get; private set; }

        public ICommand TranslatePathToEnvVarCommand { get; private set; }

        public ICommand CalculateMD5OfExistenceFilePathCommand { get; private set; }

        public RelayCommand<ICloseable> DiscardAndCloseCommand { get; private set; }

        public RelayCommand<ICloseable> SaveChangesAndCloseCommand { get; private set; }

        public ICommand BrowseToSelectSetupFileCommand { get; private set; }

        public ICommand OpenRegistryKeyCommand { get; private set; }
        public ICommand BrowseToExistenceCheckFileCommand { get; private set; }

        public SoftwareInstallStatusEnum Status
        {
            get => _status;
            set => Set(ref _status, value, nameof(Status));
        }

        public event CloseWindowRequested OnCloseWindowRequested;

        public delegate void CloseWindowRequested(SoftwareDirectoryViewModel sender, bool isSaveRequested, ICloseable window);

        #endregion Properties

        #region public methods

        public void Init()
        {
            // if QuickSetup settings file(s) exist - get the details:
            var qsFiles = _directoryInfo.GetFiles($"*.{Constants.QUICK_SETUP_SETTINGS_FILE_EXTENSION}");

            if (qsFiles.Length > 0)
            {
                OriginalModel = FilesHelper.LoadSingleSoftwareModelFromFile(qsFiles.FirstOrDefault());
                ClonedModel = new SingleSoftwareModel();

                ClonedModel.CopyDataFrom(OriginalModel);
                RaisePropertyChanged(nameof(ClonedModel));

                RefreshSoftwareInstallStatusEnum();

                if (qsFiles.Length > 1 && Debugger.IsAttached)
                {
                    Logger.Log.Warn("More than one settings file was found. this is not yet supported");
                    Debugger.Break();
                }
            }

            var subDirs = _directoryInfo.EnumerateDirectories().ToList();
            foreach (var subDir in subDirs)
            {
                var sub = new SoftwareDirectoryViewModel(subDir);
                sub.Init();
                SubDirs.Add(sub);
            }

            Logger.Log.Debug($@"Finished initializing folder {_directoryInfo.FullName}. Found QS file: {qsFiles.Length > 0}, Processed {subDirs.Count()} subFolders");
        }

        public void RefreshSoftwareInstallStatusEnum()
        {
            try
            {
                if (OriginalModel == null)
                {
                    return;
                }

                Status = SoftwareInstallStatusEnum.Unknown;

                // check file Existence - this will win Registry check
                if (!string.IsNullOrWhiteSpace(OriginalModel.ExistenceCheckFilePath))
                {
                    var path = Environment.ExpandEnvironmentVariables(OriginalModel.ExistenceCheckFilePath);

                    // file exists :-)
                    if (File.Exists(path))
                    {
                        // check hash
                        if (!string.IsNullOrWhiteSpace(OriginalModel.ExistenceCheckFileMd5Hash))
                        {
                            string strComputedHash = FilesHelper.CalculateMd5(path);

                            if (strComputedHash == OriginalModel.ExistenceCheckFileMd5Hash)
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

                // check registry value only if file existence check is not required
                if (Status == SoftwareInstallStatusEnum.Unknown &&
                    !string.IsNullOrWhiteSpace(OriginalModel.ExistenceCheckRegistryKey) &&
                    !string.IsNullOrWhiteSpace(OriginalModel.ExistenceCheckRegistryValueName))
                {
                    //http://stackoverflow.com/a/22825275/426315
                    var registryRoot = Registry.CurrentUser;
                    var strKeyToOpen = OriginalModel.ExistenceCheckRegistryKey;

                    if (OriginalModel.ExistenceCheckRegistryKey.StartsWith(Constants.HKLM, StringComparison.Ordinal))
                    {
                        registryRoot = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                        strKeyToOpen = OriginalModel.ExistenceCheckRegistryKey.Replace(Constants.HKLM + @"\", string.Empty);
                    }
                    else if (OriginalModel.ExistenceCheckRegistryKey.StartsWith(Constants.HKCU, StringComparison.Ordinal))
                    {
                        registryRoot = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                        strKeyToOpen = OriginalModel.ExistenceCheckRegistryKey.Replace(Constants.HKCU + @"\", string.Empty);
                    }

                    var registryKey = registryRoot.OpenSubKey(strKeyToOpen);

                    // if key exists
                    if (registryKey != null)
                    {
                        var regValueData = registryKey.GetValue(OriginalModel.ExistenceCheckRegistryValueName);
                        if (regValueData != null)
                        {
                            // if there's no data to compare with, we only test for key/value existence
                            if (string.IsNullOrEmpty(OriginalModel.ExistenceCheckRegistryValueData))
                            {
                                Status = SoftwareInstallStatusEnum.Installed;
                            }
                            else
                            {
                                if (regValueData is string regValueDataStr && regValueDataStr == OriginalModel.ExistenceCheckRegistryValueData ||
                                    regValueData is int regValueDataInt && regValueDataInt == Convert.ToInt32(OriginalModel.ExistenceCheckRegistryValueData))
                                {
                                    Status = SoftwareInstallStatusEnum.Installed;
                                }
                                else
                                {
                                    Status = SoftwareInstallStatusEnum.NotInstalled;
                                }
                            }
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

                if (Status == SoftwareInstallStatusEnum.Unknown &&
                    !string.IsNullOrEmpty(OriginalModel.SetupFileName) && File.Exists(Path.Combine(_directoryInfo.FullName, OriginalModel.SetupFileName)))
                {
                    Status = SoftwareInstallStatusEnum.NotInstalled;
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while getting app status", ex);
                Status = SoftwareInstallStatusEnum.UnableToGetStatus;
            }
        }

        protected virtual void RaiseCloseWindowRequested(ICloseable window, bool isSaveRequested)
        {
            OnCloseWindowRequested?.Invoke(this, isSaveRequested, window);
        }

        #endregion public methods

        #region private methods

        private void OnEditQSSettingsCommand()
        {
            try
            {
                if (OriginalModel == null)
                {
                    OriginalModel = new SingleSoftwareModel();
                    OriginalModel.AppName = _directoryInfo.Name;

                    // guess setup file name
                    var firstExe = _directoryInfo.GetFiles("*.exe", SearchOption.TopDirectoryOnly).FirstOrDefault();
                    OriginalModel.SetupFileName = firstExe?.Name;
                    RaisePropertyChanged(nameof(OriginalModel));

                    ClonedModel = new SingleSoftwareModel();
                    ClonedModel.CopyDataFrom(OriginalModel);
                    RaisePropertyChanged(nameof(ClonedModel));
                }

                Messenger.Default.Send(new NotificationMessage<SoftwareDirectoryViewModel>(this, Constants.MVVM_MESSAGE_SHOW_SINGLESOFTWAREVIEW));
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while editing Quick Setup settings", ex);
            }
        }

        private void OnInstallSoftwareCommand()
        {
            try
            {
                // TODO: handle MSI installation. avoid multiple MSI at once.

                var pi = new ProcessStartInfo();
                pi.WorkingDirectory = _directoryInfo.FullName;
                pi.FileName = Path.Combine(_directoryInfo.FullName, OriginalModel.SetupFileName);

                if (!string.IsNullOrEmpty(OriginalModel.SetupSilentParams))
                {
                    pi.Arguments = OriginalModel.SetupSilentParams;
                }

                Process.Start(pi);
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while installing", ex);
            }
        }

        private bool CanExecuteInstallSoftwareCommand()
        {
            try
            {
                if (OriginalModel == null ||
                    string.IsNullOrEmpty(OriginalModel.SetupFileName) ||
                    Status == SoftwareInstallStatusEnum.Installed)
                {
                    return false;
                }

                var strFullPath = Path.Combine(_directoryInfo.FullName, OriginalModel.SetupFileName);
                return File.Exists(strFullPath);
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while checking if setup file exists", ex);

                // we are not sure if the file exists, yet, I prefer to allow user to click it
                return true;
            }
        }

        private void OnBrowseCommand()
        {
            try
            {
                Process.Start("explorer.exe", _directoryInfo.FullName);
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while opening folder", ex);
            }
        }

        private void OnTranslatePathToEnvVarCommand()
        {
            try
            {
                var lstEnvironmentVariables = Environment.GetEnvironmentVariables();

                if (!string.IsNullOrWhiteSpace(ClonedModel.ExistenceCheckFilePath))
                {
                    foreach (DictionaryEntry envVariable in lstEnvironmentVariables)
                    {
                        if (envVariable.Key.ToString() == "HOMEDRIVE")
                        {
                            continue;
                        }

                        if (ClonedModel.ExistenceCheckFilePath.StartsWith(envVariable.Value.ToString(), StringComparison.Ordinal))
                        {
                            // replace simple string with environment variable
                            ClonedModel.ExistenceCheckFilePath =
                                ClonedModel.ExistenceCheckFilePath.Replace(envVariable.Value.ToString(),
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

        private bool CanExecuteCalculateMD5OfExistenceFilePathCommand()
        {
            return !string.IsNullOrWhiteSpace(ClonedModel.ExistenceCheckFilePath);
        }

        private void OnDiscardAndCloseCommand(ICloseable window)
        {
            RaiseCloseWindowRequested(window, false);
        }

        private void OnCalculateMD5OfExistenceFilePathCommand()
        {
            try
            {
                var path = Environment.ExpandEnvironmentVariables(ClonedModel.ExistenceCheckFilePath);

                if (File.Exists(path))
                {
                    ClonedModel.ExistenceCheckFileMd5Hash = FilesHelper.CalculateMd5(path);
                    RaisePropertyChanged(nameof(ClonedModel));
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while Translating path to environment variables", ex);
            }
        }

        private void OnSaveChangesAndCloseCommand(ICloseable window)
        {
            try
            {
                if (string.IsNullOrEmpty(ClonedModel.AppName))
                {
                    // default app name = folder name
                    ClonedModel.AppName = _directoryInfo.Name;
                }

                // copy all values to original model
                OriginalModel.CopyDataFrom(ClonedModel);
                RaisePropertyChanged(nameof(OriginalModel));

                // Delete old setting-files
                var existingSettingFiles = _directoryInfo.GetFiles("*." + Constants.QUICK_SETUP_SETTINGS_FILE_EXTENSION);
                foreach (var existingSettingFile in existingSettingFiles)
                {
                    File.Delete(existingSettingFile.FullName);
                }

                // save single file
                var targetFileName = Path.Combine(_directoryInfo.FullName, OriginalModel.AppName + "." + Constants.QUICK_SETUP_SETTINGS_FILE_EXTENSION);
                FilesHelper.SaveSingleSoftwareModelToFile(OriginalModel, targetFileName);

                // check if software is installed
                RefreshSoftwareInstallStatusEnum();
                ((RelayCommand)InstallSoftwareCommand).RaiseCanExecuteChanged();

                // notify main model that the window is closed
                RaiseCloseWindowRequested(window, true);
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while saving changes", ex);
            }

        }

        private void OnBrowseToSelectSetupFileCommand()
        {
            try
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = "Setup files (*.exe, *.msi, *.bat, *.cmd, *.vbs) | *.exe; *.msi; *.bat; *.cmd; *.vbs";
                dialog.Title = "Please select setup file.";
                dialog.InitialDirectory = _directoryInfo.FullName;
                if (dialog.ShowDialog().GetValueOrDefault(false))
                {
                    ClonedModel.SetupFileName = Path.GetFileName(dialog.FileName);
                    RaisePropertyChanged(nameof(ClonedModel));
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while getting app status", ex);
            }
        }

        private void OnBrowseToExistenceCheckFileCommand()
        {
            try
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = "Any File | *.*";
                dialog.Title = "Please select any file that marks the existence of " + OriginalModel.AppName;
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

                if (dialog.ShowDialog().GetValueOrDefault(false))
                {
                    ClonedModel.ExistenceCheckFilePath = dialog.FileName;
                    OnTranslatePathToEnvVarCommand();
                    OnCalculateMD5OfExistenceFilePathCommand();

                    RaisePropertyChanged(nameof(ClonedModel));
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while browsing to file", ex);
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
                    var arrRegPath = ClonedModel.ExistenceCheckRegistryKey.Split('\\');

                    // remove reg value to get the key alone
                    var strPathKey = ClonedModel.ExistenceCheckRegistryKey.Replace("\\" + arrRegPath.LastOrDefault(), string.Empty);
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
            return !string.IsNullOrEmpty(ClonedModel.ExistenceCheckRegistryKey);
        }

        private bool CanExecuteTranslatePathToEnvVarCommand()
        {
            return !string.IsNullOrWhiteSpace(ClonedModel.ExistenceCheckFilePath);
        }

        #endregion private methods
    }
}