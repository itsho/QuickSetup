using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net.Config;
using QuickSetup.Logic.Infra;
using QuickSetup.Logic.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using System.Windows.Input;
using System.Windows.Threading;

namespace QuickSetup.UI.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region data members

        private StringBuilder _logbuilderToScreen = null;
        private string _strLogOutput;
        private readonly DispatcherTimer _tmrLogRefresh = new DispatcherTimer();
        private SingleSoftwareViewModel _selectedSoftware;

        #endregion data members

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            SoftwareList = new ObservableCollection<SingleSoftwareViewModel>();
            SaveAllApps = new RelayCommand(OnSaveAllApps);
            LoadAllApps = new RelayCommand(OnLoadAllApps);
            AddNewAppCommand = new RelayCommand(OnAddNewAppCommand);

            // Code runs in Blend --> create design time data.
            if (IsInDesignMode)
            {
                #region design mode

                var lstRandomName = Constants.LOREM_IPSUM.Split(' ');
                var randGen = new Random();

                for (int intTemp = 0; intTemp < 4; intTemp++)
                {
                    SoftwareList.Add(new SingleSoftwareViewModel(new SingleSoftwareModel()
                    {
                        SoftwareName = lstRandomName[randGen.Next(lstRandomName.Length)],
                        NotesToolTip = Constants.LOREM_IPSUM,
                        ExistanceRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion",
                        ExistanceRegistryValue = "SM_GamesName",
                        LangCodeIso6392 = intTemp % 2 == 0 ? "eng" : "fre",
                        IsMsiSetup = intTemp % 3 == 0,
                        SetupFolder = @"\\NetworkShare\Setup\Acrobat\Reader",
                        SetupFileName = "setup.exe",
                        SetupSilentParams = "/q"
                    }));
                }

                #endregion design mode
            }
            // Code runs "for real"
            else
            {
                InitLog4NetOutputToWindow();

                SoftwareList.Clear();
                foreach (var singleModel in Dal.LoadAll())
                {
                    SoftwareList.Add(new SingleSoftwareViewModel(singleModel));
                }

#if DEBUG
                IsDev = true;
#endif
            }
        }

        #endregion Ctor

        #region Properties

        public ObservableCollection<SingleSoftwareViewModel> SoftwareList { get; set; }

        public SingleSoftwareViewModel SelectedSoftware
        {
            get { return _selectedSoftware; }
            set
            {
                Set(ref _selectedSoftware, value, nameof(SelectedSoftware));
            }
        }

        public ICommand SaveAllApps { get; private set; }

        public ICommand LoadAllApps { get; private set; }

        public ICommand AddNewAppCommand { get; private set; }

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

        private void AppeandToWindowLog(string p_strLog)
        {
            if (!string.IsNullOrWhiteSpace(p_strLog))
            {
                LogOutputToWindow += p_strLog;
                if (!p_strLog.EndsWith(Environment.NewLine, StringComparison.Ordinal))
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

        private void OnSaveAllApps()
        {
            var tempListToSave = SoftwareList.Select(appVm => appVm.ClonedModel).ToList();
            Dal.SaveAll(tempListToSave);
        }

        private void OnLoadAllApps()
        {
            SoftwareList.Clear();
            foreach (var singleModel in Dal.LoadAll())
            {
                SoftwareList.Add(new SingleSoftwareViewModel(singleModel));
            }
        }

        private void OnAddNewAppCommand()
        {
            try
            {
                var newSoft = new SingleSoftwareViewModel(new SingleSoftwareModel()
                {
                    SoftwareName = "New"
                });

                SoftwareList.Add(newSoft);

                // TODO: scroll to selected item
                Debugger.Break();
                SelectedSoftware = newSoft;

                // edit new soft
                if (SelectedSoftware.EditSoftwareCommand.CanExecute(null))
                {
                    SelectedSoftware.EditSoftwareCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while adding new software", ex);
            }
        }

        #endregion Private methods
    }
}