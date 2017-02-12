using QuickSetup.Logic.Models;
using System.Windows.Input;

namespace QuickSetup.UI.ViewModel
{
    public class SingleAppToInstallViewModel
    {
        #region Data members

        private readonly SingleAppToInstallModel _pSingleAppToInstallModel;

        #endregion Data members

        #region CTOR

        public SingleAppToInstallViewModel(SingleAppToInstallModel p_singleAppToInstallModel)
        {
            _pSingleAppToInstallModel = p_singleAppToInstallModel;
        }

        #endregion CTOR

        public ICommand InstallCommand { get; set; }

        public string AppName
        {
            get { return _pSingleAppToInstallModel.AppName; }
        }
    }
}