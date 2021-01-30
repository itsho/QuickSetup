using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using QuickSetup.UI.Infra;

namespace QuickSetup.UI.Resources.Converters
{
    public class SoftwareStatusToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var origValue = value as SoftwareInstallStatusEnum?;

                if (origValue == SoftwareInstallStatusEnum.Installed)
                {
                    return "pack://application:,,,/Resources/Icons/flaticon-success.png";
                }

                if (origValue == SoftwareInstallStatusEnum.NotInstalled)
                {
                    return "pack://application:,,,/Resources/Icons/icons8-software-24.png";
                }

                if (origValue == SoftwareInstallStatusEnum.SetupFileMissing ||
                    origValue == SoftwareInstallStatusEnum.UnableToGetStatus)
                {
                    return "pack://application:,,,/Resources/Icons/flaticon-error.png";
                }

                if (origValue == SoftwareInstallStatusEnum.DifferentVersionDetected)
                {
                    return "pack://application:,,,/Resources/Icons/icons8-opposite-opinion-24.png";
                }

                if (origValue == SoftwareInstallStatusEnum.Unknown)
                {
                    return "pack://application:,,,/Resources/Icons/icons8-folder-24.png";
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}