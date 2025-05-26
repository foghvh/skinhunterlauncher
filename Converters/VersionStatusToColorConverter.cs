using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SkinHunterLauncher.Converters
{
    public class VersionStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToUpperInvariant() switch
                {
                    "UPDATED" => Brushes.LightGreen,
                    "OUTDATED" => Brushes.OrangeRed,
                    "CHECKING..." => Brushes.LightSkyBlue,
                    _ => Brushes.LightGray,
                };
            }
            return Brushes.LightGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}