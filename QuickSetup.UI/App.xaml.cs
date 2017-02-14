using QuickSetup.Logic.Infra;
using System.Diagnostics;
using System.Windows;

namespace QuickSetup.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Logger.Setup();

            #region get version

            //http://stackoverflow.com/a/909583/426315

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

            #endregion get version

            Logger.Log.Debug("Start new run - version " + fvi.FileVersion);
            base.OnStartup(e);
        }
    }
}