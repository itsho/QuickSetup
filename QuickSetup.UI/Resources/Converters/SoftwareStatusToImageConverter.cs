using QuickSetup.Logic.Infra;
using QuickSetup.Logic.Infra.Enums;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

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
                    return "pack://application:,,,/Resources/Icons-flaticon/success.png";
                }
                if (origValue == SoftwareInstallStatusEnum.NotInstalled)
                {
                    return "pack://application:,,,/Resources/Icons-flaticon/circumference1.png";
                }
                if (origValue == SoftwareInstallStatusEnum.SetupFileMissing ||
                    origValue == SoftwareInstallStatusEnum.UnableToGetStatus)
                {
                    return "pack://application:,,,/Resources/Icons-flaticon/error.png";
                }

                //if (origValue == SoftwareInstallStatusEnum.Unknown)
                //{
                //    return "pack://application:,,,/Resources/Icons-flaticon/question.png";
                //}
                return DependencyProperty.UnsetValue;
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}