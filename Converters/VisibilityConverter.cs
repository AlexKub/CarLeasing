using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CarLeasingViewer.Converters
{
    /// <summary>
    /// Измнение видимости в зависимости от флага
    /// </summary>
    public class VisibilityConverter : IValueConverter
    {
        public Visibility OnTrue { get; set; } = Visibility.Visible;

        public Visibility OnFalse { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                return OnFalse;

            var val = ((bool)value) ? OnTrue : OnFalse;

            return val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Visibility))
                return false;

            if ((Visibility)value == OnTrue)
                return true;

            return false;

        }
    }
}
