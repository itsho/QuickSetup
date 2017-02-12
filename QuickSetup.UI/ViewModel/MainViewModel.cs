using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using QuickSetup.Logic.Infra;
using QuickSetup.Logic.Models;

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
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ListOfApps = new ObservableCollection<SingleAppToInstallViewModel>();
            SaveAllApps = new RelayCommand(OnSaveAllApps);
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                ListOfApps.Add(new SingleAppToInstallViewModel(new SingleAppToInstallModel()
                {
                    AppName = "App1",
                }));
            }
            else
            {
                // Code runs "for real"
                Dal.LoadAll();
            }
        }

        private void OnSaveAllApps()
        {
            Dal.SaveAll();
        }
        private void OnLoadAllApps()
        {
            Dal.LoadAll();
        }
        public ObservableCollection<SingleAppToInstallViewModel> ListOfApps { get; set; }

        public ICommand SaveAllApps { get; private set; }
        public ICommand LoadAllApps { get; private set; }
    }
}