using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.IO;
using System.Reflection;
using log4net.Core;

namespace QuickSetup.Logic.Infra
{
    public static class Logger
    {
        #region Data members

        private static bool _blnIsInitialized = false;

        #endregion Data members

        #region Properties

        public static ILog Log { get; } = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion Properties

        public static void Setup()
        {
            if (_blnIsInitialized)
            {
                return;
            }

            PatternLayout patternLayoutFull = new PatternLayout();
            patternLayoutFull.ConversionPattern = "%date  [%thread]  |%level|  |%class.%method() line: %line|  || %message %newline";
            patternLayoutFull.ActivateOptions();

            RollingFileAppender roller = new RollingFileAppender();
            roller.Layout = patternLayoutFull;

            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            #region File logger

            roller.AppendToFile = true;

            roller.File = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                string.Format(@"{0}\{1}.txt", Constants.APPLICATIONNAME, Constants.APPLICATIONNAME));
            roller.RollingStyle = RollingFileAppender.RollingMode.Composite;
            roller.MaxSizeRollBackups = 5;
            roller.MaximumFileSize = "20MB";
            roller.StaticLogFileName = true;
            roller.LockingModel = new FileAppender.MinimalLock();
            roller.ActivateOptions();

            hierarchy.Root.AddAppender(roller);

            #endregion File logger

            //#region Windows Event Logger

            //var eventLoggerAppender = new EventLogAppender
            //{
            //    ApplicationName = Constants.APPLICATIONNAME,
            //    LogName = Constants.APPLICATIONNAME,
            //    Layout = patternLayout,
            //    Threshold = Level.Error
            //};
            //eventLoggerAppender.ActivateOptions();
            //hierarchy.Root.AddAppender(eventLoggerAppender);

            //#endregion Windows Event Logger

            if (Environment.UserInteractive)
            {
                // we write to console and capture it later to a WPF window
                //so, we only write the parts the end-user like to see
                var patternLayoutPartial = new PatternLayout();
                patternLayoutPartial.ConversionPattern = "%date  [%thread]  |%level|  || %message %newline";
                patternLayoutPartial.ActivateOptions();

                var consoleAppender = new ConsoleAppender
                {
                    Name = Constants.APPLICATIONNAME,
                    Layout = patternLayoutPartial,
                    Threshold = Level.All
                };
                consoleAppender.ActivateOptions();
                hierarchy.Root.AddAppender(consoleAppender);
            }

            hierarchy.Configured = true;

            _blnIsInitialized = true;
        }
    }
}