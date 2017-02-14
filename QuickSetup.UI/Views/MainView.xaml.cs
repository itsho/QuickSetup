using log4net.Config;
using MahApps.Metro.Controls;
using QuickSetup.Logic.Infra;
using System;
using System.IO;
using System.Text;
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
        }

        private void CommandBinding_OnExecuted(object p_sender, ExecutedRoutedEventArgs p_e)
        {
            Close();
        }

        private void ButtonInstall_OnClick(object p_sender, RoutedEventArgs p_e)
        {
            try
            {
                Logger.Log.Debug("ButtonInstall_OnClick started");
                Cell cell = Cell.FindFromChild(p_sender as DependencyObject);
                var drv2 = DataGridControl.GetDataGridContext(cell).GetItemFromContainer(cell.ParentRow) as SingleAppToInstallView;

                //var editor = new ProductsEditorWindow();
                //editor.ShowDialog();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }
        }

        private void ButtonEdit_OnClick(object p_sender, RoutedEventArgs p_e)
        {
            try
            {
                Logger.Log.Debug("ButtonEdit_OnClick started");
                Cell cell = Cell.FindFromChild(p_sender as DependencyObject);
                var drv2 = DataGridControl.GetDataGridContext(cell).GetItemFromContainer(cell.ParentRow) as SingleAppToInstallView;

                //var editor = new ProductsEditorWindow();
                //editor.ShowDialog();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }
        }
    }
}