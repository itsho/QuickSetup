using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net.Config;
using Microsoft.WindowsAPICodePack.Dialogs;
using QuickSetup.Logic.Infra;
using QuickSetup.Logic.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;

namespace QuickSetup.UI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region data members

        private StringBuilder _logbuilderToScreen = null;
        private string _strLogOutput;
        private readonly DispatcherTimer _tmrLogRefresh = new DispatcherTimer();
        private SoftwareDirectoryViewModel _selectedSoftwareFolder;
        private AppSettings _qsSettings;

        #endregion data members

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            FoldersList = new ObservableCollection<SoftwareDirectoryViewModel>();
            ScanFolderCommand = new RelayCommand(OnScanFolderCommand);
            BrowseWorkingFolderCommand = new RelayCommand(OnBrowseWorkingFolderCommand);
            ClearRecentWorkingFolderCommand = new RelayCommand(OnClearRecentWorkingFolderCommand);

            QSSettings = new AppSettings();
            ShowAllFolders = true;

            // Code runs in Blend --> create design time data.
            if (IsInDesignMode)
            {
                #region design mode

                //var sub = new SoftwareDirectoryViewModel(QSSettings.WorkingFolder);
                //sub.Init();
                //FoldersList.Add(sub);

                //var lstRandomName = Constants.LOREM_IPSUM.Split(' ');
                //var randGen = new Random();
                //var lstPossibleCategories = new List<string>() { "Documents", "Graphics", "Dev" };

                //SoftwareList.Add(new SingleSoftwareViewModel(new SingleSoftwareModel()
                //{
                //    SoftwareName = lstRandomName[randGen.Next(lstRandomName.Length)],
                //    NotesToolTip = Constants.LOREM_IPSUM,
                //    ExistenceRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion",
                //    ExistenceRegistryValue = "SM_GamesName",
                //    LangCodeIso6392 = intTemp % 2 == 0 ? "eng" : "fre",
                //    IsMsiSetup = intTemp % 3 == 0,
                //    SetupFolder = @"\\NetworkShare\Setup\Acrobat\Reader",
                //    SetupFileName = "setup.exe",
                //    SetupSilentParams = "/q"
                //},
                //lstPossibleCategories));

                #endregion design mode
            }
            // Code runs "for real"
            else
            {
                InitLog4NetOutputToWindow();
                LoadAppSettings();
                ScanFolderCommand.Execute(null);
            }
        }

        #endregion Ctor

        #region Properties

        public ObservableCollection<SoftwareDirectoryViewModel> FoldersList { get; private set; }

        public SoftwareDirectoryViewModel SelectedSoftwareFolder
        {
            get { return _selectedSoftwareFolder; }
            set
            {
                if (_selectedSoftwareFolder != value)
                {
                    _selectedSoftwareFolder = value;
                    RaisePropertyChanged();
                }
            }
        }

        public AppSettings QSSettings
        {
            get { return _qsSettings; }
            private set
            {
                if (_qsSettings != value)
                {
                    _qsSettings = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(ShowAllFolders));
                    RaisePropertyChanged(nameof(WorkingFolder));
                    RaisePropertyChanged(nameof(QSSettings.RecentWorkingFolders));
                }
            }
        }

        public bool ShowAllFolders
        {
            get { return QSSettings.IsShowAllFolders; }
            set
            {
                if (QSSettings.IsShowAllFolders != value)
                {
                    QSSettings.IsShowAllFolders = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string WorkingFolder
        {
            get { return QSSettings.WorkingFolder; }
            set
            {
                if (QSSettings.WorkingFolder != value)
                {
                    QSSettings.WorkingFolder = value;
                    RaisePropertyChanged();

                    if (!string.IsNullOrEmpty(QSSettings.WorkingFolder))
                    {
                        SaveAppSettings();
                    }
                }
            }
        }

        public ICommand ScanFolderCommand { get; private set; }

        public ICommand BrowseWorkingFolderCommand { get; private set; }

        public ICommand ClearRecentWorkingFolderCommand { get; private set; }

        public string LogOutputToWindow
        {
            get { return _strLogOutput; }
            set { Set(ref _strLogOutput, value, nameof(LogOutputToWindow)); }
        }

        #endregion Properties

        #region Private methods

        /// <summary>
        /// based on
        /// https://www.roelvanlisdonk.nl/2012/05/11/how-to-redirect-the-standard-console-output-to-assert-logmessages-written-by-log4net/
        /// </summary>
        private void InitLog4NetOutputToWindow()
        {
            try
            {
                // no need to Save original console output writer.
                //var originalConsole = Console.Out;

                // Configure log4net based on the App.config
                XmlConfigurator.Configure();
                _logbuilderToScreen = new StringBuilder();
                var logWriterToScreen = new StringWriter(_logbuilderToScreen);

                // Redirect all Console messages to the StringWriter.
                Console.SetOut(logWriterToScreen);

                // Log a message.
                Logger.Log.Debug("New Run is starting...");

                // Get all messages written to the console.
                _tmrLogRefresh.Interval = TimeSpan.FromMilliseconds(500);
                _tmrLogRefresh.Tick += _tmrLogRefresh_Tick;
                _tmrLogRefresh.Start();

                // Redirect back to original console output.
                // Console.SetOut(originalConsole);
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Unable to capture log to screen", ex);
                AppeandToWindowLog("Unable to capture log to screen. Log won't be availble.");
            }
        }

        private void _tmrLogRefresh_Tick(object sender, EventArgs e)
        {
            GetLog4NetOutputChunk();
        }

        private void AppeandToWindowLog(string log)
        {
            if (!string.IsNullOrWhiteSpace(log))
            {
                LogOutputToWindow += log;
                if (!log.EndsWith(Environment.NewLine, StringComparison.Ordinal))
                {
                    LogOutputToWindow += Environment.NewLine;
                }
            }
        }

        private void GetLog4NetOutputChunk()
        {
            try
            {
                string strConsoleOutput = string.Empty;
                using (var reader = new StringReader(_logbuilderToScreen.ToString()))
                {
                    strConsoleOutput = reader.ReadToEnd();
                }

                // clear previous chuck
                _logbuilderToScreen.Clear();

                // display the chunk we just got
                AppeandToWindowLog(strConsoleOutput);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }
        }

        private void OnScanFolderCommand()
        {
            try
            {
                FoldersList.Clear();

                if (string.IsNullOrEmpty(WorkingFolder))
                {
                    Logger.Log.Warn("Working folder was not selected - nothing to scan");
                    return;
                }

                if (!Directory.Exists(WorkingFolder))
                {
                    Logger.Log.Warn("Selected Working folder is invalid - unable to scan");
                    return;
                }

                // get folders in root folder
                var rootSubFolders = Directory.GetDirectories(WorkingFolder, "*.", SearchOption.TopDirectoryOnly);

                // create hierarchical folders list
                foreach (var subFolder in rootSubFolders)
                {
                    var sub = new SoftwareDirectoryViewModel(subFolder);
                    if (sub.Init(ShowAllFolders))
                    {
                        FoldersList.Add(sub);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }
        }

        private void OnClearRecentWorkingFolderCommand()
        {
            try
            {
                QSSettings.RecentWorkingFolders.Clear();
                SaveAppSettings();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }
        }

        private void OnBrowseWorkingFolderCommand()
        {
            try
            {
                var dialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = true,
                    InitialDirectory = WorkingFolder
                };
                var res = dialog.ShowDialog();
                if (res == CommonFileDialogResult.Ok)
                {
                    if (!_qsSettings.RecentWorkingFolders.Contains(dialog.FileName))
                    {
                        _qsSettings.RecentWorkingFolders.Add(dialog.FileName);
                    }

                    WorkingFolder = dialog.FileName;
                    RaisePropertyChanged(nameof(WorkingFolder));

                    ScanFolderCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }
        }

        private void SaveAppSettings()
        {
            try
            {
                FilesHelper.SaveAppSettingsToFile(QSSettings);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }
        }

        private void LoadAppSettings()
        {
            try
            {
                QSSettings = FilesHelper.LoadAppSettingsFromFile();
                RaisePropertyChanged(nameof(ShowAllFolders));
                RaisePropertyChanged(nameof(WorkingFolder));
                RaisePropertyChanged(nameof(QSSettings.RecentWorkingFolders));
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }
        }

        #endregion Private methods
    }
}