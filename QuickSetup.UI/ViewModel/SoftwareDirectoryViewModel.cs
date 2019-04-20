using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using QuickSetup.Logic.Infra;
using QuickSetup.Logic.Infra.Enums;
using QuickSetup.Logic.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace QuickSetup.UI.ViewModel
{
    public class SoftwareDirectoryViewModel : ViewModelBase
    {
        #region Data members

        private readonly DirectoryInfo _directoryInfo;

        private SoftwareInstallStatusEnum _status;
        private SingleSoftwareModel _softwareModel;

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
            BrowseCommand = new RelayCommand(OnBrowseCommand);
        }

        #endregion CTOR

        #region Properties

        public List<SoftwareDirectoryViewModel> SubDirs { get; private set; }

        public string Name
        {
            get { return _directoryInfo.Name; }
        }

        public RelayCommand InstallSoftwareCommand { get; private set; }
        public RelayCommand EditQSSettingsCommand { get; private set; }
        public RelayCommand BrowseCommand { get; private set; }

        public SoftwareInstallStatusEnum Status
        {
            get => _status;
            set => Set(ref _status, value, nameof(Status));
        }

        #endregion Properties

        #region public methods

        public void Init()
        {
            // if QuickSetup settings file(s) exist - get the details:
            var qsFiles = _directoryInfo.GetFiles($"*.{Constants.QUICK_SETUP_SETTINGS_FILE_EXTENSION}");

            if (qsFiles.Length > 0)
            {
                _softwareModel = SingleSoftwareModel.LoadFromFile(qsFiles.FirstOrDefault());
                


                if (qsFiles.Length > 1 && Debugger.IsAttached)
                {
                    Logger.Log.Warn("More than one settings file was found. this is not yet supported");
                    Debugger.Break();
                }
            }
            

            foreach (var subDir in _directoryInfo.EnumerateDirectories())
            {
                var sub = new SoftwareDirectoryViewModel(subDir);
                sub.Init();
                SubDirs.Add(sub);
            }
        }

        public void RefreshSoftwareInstallStatusEnum()
        {
            try
            {
                Status = SoftwareInstallStatusEnum.Unknown;

                // check registry value
                if (!string.IsNullOrWhiteSpace(_softwareModel.ExistenceCheckRegistryKey) &&
                    !string.IsNullOrWhiteSpace(_softwareModel.ExistenceCheckRegistryValue))
                {
                    //http://stackoverflow.com/a/22825275/426315
                    var registryRoot = Registry.CurrentUser;
                    var strKeyToOpen = _softwareModel.ExistenceCheckRegistryKey;

                    if (_softwareModel.ExistenceCheckRegistryKey.StartsWith(Constants.HKLM, StringComparison.Ordinal))
                    {
                        registryRoot = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                        strKeyToOpen = _softwareModel.ExistenceCheckRegistryKey.Replace(Constants.HKLM + @"\", string.Empty);
                    }
                    else if (_softwareModel.ExistenceCheckRegistryKey.StartsWith(Constants.HKCU, StringComparison.Ordinal))
                    {
                        registryRoot = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                        strKeyToOpen = _softwareModel.ExistenceCheckRegistryKey.Replace(Constants.HKCU + @"\", string.Empty);
                    }

                    var registryKey = registryRoot.OpenSubKey(strKeyToOpen);

                    // if key exists
                    if (registryKey != null)
                    {
                        var strValue = registryKey.GetValue(_softwareModel.ExistenceCheckRegistryValue) as string;
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

                // check file Existence - this will override Registry check
                if (!string.IsNullOrWhiteSpace(_softwareModel.ExistenceCheckFilePath))
                {
                    var path = Environment.ExpandEnvironmentVariables(_softwareModel.ExistenceCheckFilePath);

                    // file exists :-)
                    if (File.Exists(path))
                    {
                        // check hash
                        if (!string.IsNullOrWhiteSpace(_softwareModel.ExistenceCheckFileMd5Hash))
                        {
                            string strComputedHash = FilesHelper.CalculateMd5(path);

                            if (strComputedHash == _softwareModel.ExistenceCheckFileMd5Hash)
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

        #endregion public methods

        #region private methods

        private void OnEditQSSettingsCommand()
        {   
            Debugger.Break();
            
        }

        private void OnInstallSoftwareCommand()
        {
            try
            {
                
                var strArgument = @"/select, """ + Path.Combine(_directoryInfo.FullName, _softwareModel.SetupFileName) + @"""";
                Process.Start("explorer.exe", strArgument);
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
                var strFullPath = Path.Combine(_directoryInfo.FullName, _softwareModel.SetupFileName);
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
            Debugger.Break();
        }

        #endregion private methods
    }
}