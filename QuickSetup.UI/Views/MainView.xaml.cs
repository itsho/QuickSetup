using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using QuickSetup.UI.Infra;
using QuickSetup.UI.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuickSetup.UI.Views
{
    public partial class MainView : MetroWindow, ICloseable
    {
        public MainView()
        {
            InitializeComponent();
            Messenger.Default.Register<NotificationMessage<SoftwareDirectoryViewModel>>(this, ProcessMessagesFromSoftwareDirectoryViewModel);
            Messenger.Default.Register<NotificationMessage<MainViewModel>>(this, ProcessMessagesFromMainViewModel);
        }

        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void OnContentOnOnCloseWindowRequested(SoftwareDirectoryViewModel sender, bool isSaveRequested, ICloseable window)
        {
            sender.OnCloseWindowRequested -= OnContentOnOnCloseWindowRequested;

            // if user clicked on save
            if (isSaveRequested)
            {
                if (DataContext is MainViewModel mvm)
                {
                    // TODO: reload the folder tree icon and text?.

                    //Debugger.Break();
                }
            }

            window.Close();
        }

        private void ProcessMessagesFromSoftwareDirectoryViewModel(NotificationMessage<SoftwareDirectoryViewModel> notificationMessage)
        {
            try
            {
                if (notificationMessage.Notification == Constants.MVVM_MESSAGE_SHOW_SINGLESOFTWAREVIEW)
                {
                    if (notificationMessage.Content.SubDirs.Count > 0)
                    {
                        var res = MessageBox.Show("It is not recommended to create setup from a PARENT folder." +
                                                  Environment.NewLine +
                                                  "Do you want to continue?",
                            Constants.APPLICATIONNAME,
                            MessageBoxButton.YesNo);
                        if (res == MessageBoxResult.No)
                        {
                            return;
                        }
                    }

                    var ssv = new SingleSoftwareView();
                    ssv.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    ssv.DataContext = notificationMessage.Content;
                    notificationMessage.Content.OnCloseWindowRequested += OnContentOnOnCloseWindowRequested;

                    //ssv.Parent = this;
                    ssv.Owner = this;
                    ssv.ShowDialog();

                    // refresh current item
                    var vm = DataContext as MainViewModel;
                    vm?.RaisePropertyChanged(nameof(vm.SelectedSoftwareFolder));
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while mvvm message received", ex);
            }
        }

        private void ProcessMessagesFromMainViewModel(NotificationMessage<MainViewModel> notificationMessage)
        {
            try
            {
                if (notificationMessage.Notification == Constants.MVVM_MESSAGE_SHOW_ABOUTVIEW)
                {
                    var av = new AboutView();
                    av.Owner = this;
                    av.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while mvvm message received", ex);
            }
        }

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tree = (TreeView)sender;
            if (tree.SelectedItem is SoftwareDirectoryViewModel softwareDirectoryVm)
            {
                if (DataContext is MainViewModel mvm)
                {
                    mvm.SelectedSoftwareFolder = softwareDirectoryVm;
                }
            }
        }
    }
}