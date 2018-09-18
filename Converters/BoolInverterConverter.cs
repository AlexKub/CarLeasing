using System;
using System.Globalization;
using System.Windows.Data;

namespace CarLeasingViewer.Converters
{
    public class BoolInverterConverter : IValueConverter
    {
        public bool Default { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is bool))
                return Default;

            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is bool))
                return Default;

            return !((bool)value);
        }
    }
}
