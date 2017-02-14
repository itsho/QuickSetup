using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using QuickSetup.Logic.Infra;
using QuickSetup.Logic.Infra.Enums;
using QuickSetup.Logic.Models;
using System;
using System.IO;
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

        private bool _isChecked;
        private SoftwareInstallStatusEnum _status;

        #endregion Data members

        #region CTOR

        public SingleSoftwareViewModel(SingleSoftwareModel p_singleSoftwareModel)
        {
            Model = p_singleSoftwareModel;
            InstallCommand = new RelayCommand(OnInstallCommand, CanExecuteInstallCommand);
            EditAppCommand = new RelayCommand(OnEditAppCommand, CanExecuteEditAppCommand);

            RefreshSoftwareInstallStatusEnum();
        }

        #endregion CTOR

        #region Properties

        public SingleSoftwareModel Model { get; private set; }

        public ICommand InstallCommand { get; set; }

        public ICommand EditAppCommand { get; set; }

        public SoftwareInstallStatusEnum Status
        {
            get { return _status; }
            set { Set(ref _status, value, nameof(Status)); }
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set { Set(ref _isChecked, value, nameof(IsChecked)); }
        }

        #endregion Properties

        #region Private methods

        private void OnEditAppCommand()
        {
        }

        private bool CanExecuteEditAppCommand()
        {
            return true;
        }

        private void OnInstallCommand()
        {
        }

        private bool CanExecuteInstallCommand()
        {
            return true;
        }

        public void RefreshSoftwareInstallStatusEnum()
        {
            try
            {
                Status = SoftwareInstallStatusEnum.Unknown;

                // check registry value
                if (!string.IsNullOrWhiteSpace(Model.ExistanceRegistryKey) &&
                    !string.IsNullOrWhiteSpace(Model.ExistanceRegistryValue))
                {
                    //http://stackoverflow.com/a/22825275/426315
                    var registryRoot = Registry.CurrentUser;
                    var strKeyToOpen = Model.ExistanceRegistryKey;

                    if (Model.ExistanceRegistryKey.StartsWith(HKLM, StringComparison.Ordinal))
                    {
                        registryRoot = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                        strKeyToOpen = Model.ExistanceRegistryKey.Replace(HKLM + @"\", string.Empty);
                    }
                    else if (Model.ExistanceRegistryKey.StartsWith(HKCU, StringComparison.Ordinal))
                    {
                        registryRoot = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                        strKeyToOpen = Model.ExistanceRegistryKey.Replace(HKCU + @"\", string.Empty);
                    }

                    var registryKey = registryRoot.OpenSubKey(strKeyToOpen);

                    // if key exists
                    if (registryKey != null)
                    {
                        var strValue = registryKey.GetValue(Model.ExistanceRegistryValue) as string;
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
                if (!string.IsNullOrWhiteSpace(Model.ExistanceFilePath))
                {
                    var path = Environment.ExpandEnvironmentVariables(Model.ExistanceFilePath);

                    // file exists :-)
                    if (File.Exists(path))
                    {
                        // check hash
                        if (!string.IsNullOrWhiteSpace(Model.ExistanceFileMd5Hash))
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

                            if (strComputedHash == Model.ExistanceFileMd5Hash)
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

        #endregion Private methods
    }
}