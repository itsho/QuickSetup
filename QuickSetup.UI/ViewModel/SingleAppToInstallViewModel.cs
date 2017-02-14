using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using QuickSetup.Logic.Models;
using System.Windows.Input;

namespace QuickSetup.UI.ViewModel
{
    public class SingleAppToInstallViewModel : ViewModelBase
    {
        #region Data members

        private bool _isChecked;
        private string _status;

        #endregion Data members

        #region CTOR

        public SingleAppToInstallViewModel(SingleAppToInstallModel p_singleAppToInstallModel)
        {
            Model = p_singleAppToInstallModel;
            InstallCommand = new RelayCommand(OnInstallCommand, CanExecuteInstallCommand);
            EditAppCommand = new RelayCommand(OnEditAppCommand, CanExecuteEditAppCommand);
        }

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

        #endregion CTOR

        #region Properties

        public SingleAppToInstallModel Model { get; private set; }

        public ICommand InstallCommand { get; set; }

        public ICommand EditAppCommand { get; set; }

        public string Status
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
    }
}