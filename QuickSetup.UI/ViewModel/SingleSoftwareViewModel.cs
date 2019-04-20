using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using QuickSetup.Logic.Infra;
using QuickSetup.Logic.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace QuickSetup.UI.ViewModel
{
    public class SingleSoftwareViewModel : ViewModelBase
    {
        #region Data members

        private bool _blnIsChecked;

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

            EditSoftwareCommand = new RelayCommand(OnEditSoftwareCommand, () => true);
            TranslatePathToEnvVarCommand = new RelayCommand(OnTranslatePathToEnvVarCommand, CanExecuteTranslatePathToEnvVarCommand);
            CalculateMD5OfExistenceFilePathCommand = new RelayCommand(OnCalculateMD5OfExistenceFilePathCommand, CanExecuteCalculateMD5OfExistenceFilePathCommand);
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

        public string SoftwareName => OriginalModel.AppName;
        public string NotesToolTip => OriginalModel.NotesToolTip;

        public Dictionary<string, string> ListOfIso6392 { get; private set; }

        public List<string> PossibleCategories { get; private set; }

        public ICommand InstallCommand { get; private set; }

        public ICommand EditSoftwareCommand { get; private set; }

        public ICommand TranslatePathToEnvVarCommand { get; private set; }

        public ICommand CalculateMD5OfExistenceFilePathCommand { get; private set; }

        public ICommand DiscardAndCloseCommand { get; private set; }

        public ICommand SaveChangesAndCloseCommand { get; private set; }

        public ICommand BrowseToSelectSetupFileCommand { get; private set; }

        public ICommand OpenRegistryKeyCommand { get; private set; }

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

      

        private bool CanExecuteTranslatePathToEnvVarCommand()
        {
            return !string.IsNullOrWhiteSpace(ClonedModel.ExistenceCheckFilePath);
        }

        private void OnTranslatePathToEnvVarCommand()
        {
            try
            {
                var lstEnvironmentVariables = Environment.GetEnvironmentVariables();

                Debugger.Break();
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

        private void RefreshSoftwareInstallStatusEnum()
        {
            throw new NotImplementedException();
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

        #endregion Private methods

        #region public methods

        protected virtual void RaiseCloseWindowRequested(bool p_blnIsSaveRequested)
        {
            OnCloseWindowRequested?.Invoke(p_blnIsSaveRequested);
        }

        #endregion public methods
    }
}