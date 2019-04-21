using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using QuickSetup.Logic.Infra;
using QuickSetup.UI.Infra;
using QuickSetup.UI.ViewModel;
using System;
using System.Diagnostics;
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
            Messenger.Default.Register<NotificationMessage<SoftwareDirectoryViewModel>>(this, ShowSoftwareDirectoryView);
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
                    // TODO: reload the folder tree or at least the selected item.
                     
                    //Debugger.Break();
                }
            }

            window.Close();
        }

        private void ShowSoftwareDirectoryView(NotificationMessage<SoftwareDirectoryViewModel> notificationMessage)
        {
            try
            {
                if (notificationMessage.Notification == Constants.MVVM_MESSAGE_SHOW_SINGLESOFTWAREVIEW)
                {
                    var ssv = new SingleSoftwareView();
                    ssv.DataContext = notificationMessage.Content;
                    notificationMessage.Content.OnCloseWindowRequested += OnContentOnOnCloseWindowRequested;

                    //ssv.Parent = this;
                    ssv.ShowDialog();
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