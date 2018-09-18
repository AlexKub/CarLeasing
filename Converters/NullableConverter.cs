using System;
using System.Globalization;
using System.Windows.Data;

namespace CarLeasingViewer.Converters
{
    public class NullableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            if(value is bool?)
            {
                return ((bool?)value).Value;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new bool?(false);

            if (value is bool)
                return new bool?((bool)value);

            return default(bool?);
        }
    }
}
