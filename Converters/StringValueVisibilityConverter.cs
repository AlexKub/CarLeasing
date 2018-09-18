using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CarLeasingViewer.Converters
{
    public class StringValueVisibilityConverter : IValueConverter
    {
        public Visibility OnNull { get; set; } = Visibility.Collapsed;

        public Visibility OnEmpty { get; set; } = Visibility.Collapsed;

        public Visibility OnFilled { get; set; } = Visibility.Visible;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return OnNull;

            var str = value as string;

            if (str == null)
                return OnNull;

            if (string.IsNullOrEmpty(str))
                return OnEmpty;

            return OnFilled;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
