using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using QuickSetup.Logic.Infra;
using QuickSetup.UI.ViewModel;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.DataGrid;

namespace QuickSetup.UI.Views
{
    public partial class MainView : MetroWindow
    {
        public MainView()
        {
            InitializeComponent();
            Messenger.Default.Register<NotificationMessage>(this, NotificationMessageReceived);
        }

        private void CommandBinding_OnExecuted(object p_sender, ExecutedRoutedEventArgs p_e)
        {
            Close();
        }

        private void ButtonInstall_OnClick(object p_sender, RoutedEventArgs p_e)
        {
            try
            {
                Logger.Log.Debug("Install button clicked");
                var cell = Cell.FindFromChild(p_sender as DependencyObject);
                var singleApp = DataGridControl.GetDataGridContext(cell).GetItemFromContainer(cell.ParentRow) as SingleSoftwareViewModel;

                if (singleApp != null && singleApp.InstallCommand.CanExecute(null))
                {
                    singleApp.InstallCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while trying to install software", ex);
            }
        }

        private void ButtonEdit_OnClick(object p_sender, RoutedEventArgs p_e)
        {
            try
            {
                Logger.Log.Debug("Edit button clicked");
                var cell = Cell.FindFromChild(p_sender as DependencyObject);
                var singleApp = DataGridControl.GetDataGridContext(cell).GetItemFromContainer(cell.ParentRow) as SingleSoftwareViewModel;

                if (singleApp != null && singleApp.EditSoftwareCommand.CanExecute(null))
                {
                    singleApp.EditSoftwareCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while trying to edit software", ex);
            }
        }

        private void NotificationMessageReceived(NotificationMessage p_notificationMessage)
        {
            try
            {
                if (p_notificationMessage.Notification == Constants.MVVM_MESSAGE_SHOW_SINGLESOFTWAREVIEW)
                {
                    var ssv = new SingleSoftwareView();
                    ssv.DataContext = p_notificationMessage.Sender;
                    var singleSoftwareViewModel = p_notificationMessage.Sender as SingleSoftwareViewModel;
                    if (singleSoftwareViewModel != null)
                    {
                        singleSoftwareViewModel.OnCloseWindowRequested += (p_blnIsSaveRequested) =>
                        {
                            // if user clicked on save
                            if (p_blnIsSaveRequested)
                            {
                                var mvm = DataContext as MainViewModel;
                                if (mvm != null)
                                {
                                    if (mvm.SaveAllApps.CanExecute(null))
                                    {
                                        mvm.SaveAllApps.Execute(null);
                                    }
                                }
                            }
                            ssv.Close();
                        };
                    }

                    //ssv.Owner = this;
                    ssv.Show();
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while mvvm message recieved", ex);
            }
        }

        private void ButtonRemove_OnClick(object p_sender, RoutedEventArgs p_e)
        {
            try
            {
                Logger.Log.Debug("Remove button clicked");

                var res = MessageBox.Show("Are you sure you want to remove the software?", Constants.APPLICATIONNAME,
                    MessageBoxButton.YesNo);

                if (res == MessageBoxResult.Yes)
                {
                    var cell = Cell.FindFromChild(p_sender as DependencyObject);
                    var singleApp =
                        DataGridControl.GetDataGridContext(cell).GetItemFromContainer(cell.ParentRow) as
                            SingleSoftwareViewModel;

                    var mvm = DataContext as MainViewModel;
                    if (mvm != null)
                    {
                        mvm.SoftwareList.Remove(singleApp);
                        mvm.SelectedSoftware = mvm.SoftwareList.FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while trying to Remove software", ex);
            }
        }
    }
}