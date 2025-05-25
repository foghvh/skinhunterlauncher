// SkinHunterLauncher/Converters/BooleanToVisibilityConverter.cs
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SkinHunterLauncher.Converters // Asegúrate que este namespace es correcto
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            if (value is bool bValue)
            {
                flag = bValue;
            }

            // Considera si el parámetro "Inverse" es necesario aquí,
            // si no, puedes simplificar esta lógica.
            // Si no pasas "Inverse" como ConverterParameter, esta parte no se usa.
            bool inverse = false;
            if (parameter is string paramString)
            {
                bool.TryParse(paramString, out inverse);
            }


            if (inverse)
            {
                return !flag ? Visibility.Visible : Visibility.Collapsed;
            }
            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverse = false;
            if (parameter is string paramString)
            {
                bool.TryParse(paramString, out inverse);
            }

            bool flag = (value is Visibility v) && v == Visibility.Visible;

            if (inverse)
            {
                return !flag;
            }
            return flag;
        }
    }
}