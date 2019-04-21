using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net.Config;
using QuickSetup.Logic.Infra;
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

        private string _workingFolder = null;
        private StringBuilder _logbuilderToScreen = null;
        private string _strLogOutput;
        private readonly DispatcherTimer _tmrLogRefresh = new DispatcherTimer();
        private SoftwareDirectoryViewModel _selectedSoftwareFolder;
        private bool _showAllFolders;

        #endregion data members

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            FoldersList = new ObservableCollection<SoftwareDirectoryViewModel>();

            ScanFolderCommand = new RelayCommand(OnScanFolderCommand);

            //var currFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            WorkingFolder = @"D:\Standard";
            ShowAllFolders = true;

            // Code runs in Blend --> create design time data.
            if (IsInDesignMode)
            {
                #region design mode

                var sub = new SoftwareDirectoryViewModel(WorkingFolder);
                sub.Init();
                FoldersList.Add(sub);

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

#if DEBUG
                IsDev = true;
#endif
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

        public bool ShowAllFolders
        {
            get { return _showAllFolders; }
            set
            {
                if (_showAllFolders != value)
                {
                    _showAllFolders = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string WorkingFolder
        {
            get { return _workingFolder; }
            set
            {
                if (_workingFolder != value)
                {
                    _workingFolder = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand ScanFolderCommand { get; private set; }

        public string LogOutputToWindow
        {
            get { return _strLogOutput; }
            set { Set(ref _strLogOutput, value, nameof(LogOutputToWindow)); }
        }

        public bool IsDev { get; set; }

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

                // get folders in root folder
                var rootSubFolders = Directory.GetDirectories(WorkingFolder, "*.", SearchOption.TopDirectoryOnly);

                // create hierarchical folders list
                foreach (var subFolder in rootSubFolders)
                {
                    var sub = new SoftwareDirectoryViewModel(subFolder);
                    sub.Init();
                    FoldersList.Add(sub);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }
        }

        #endregion Private methods
    }
}