using System;
using System.Globalization;
using System.Windows.Data;

namespace CarLeasingViewer.Converters
{
    public class DBSearchTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (DBSearchType)value;

            return val.GetDescription();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DBSearchTypeHelper.DefaultValue;

            return DBSearchTypeHelper.GetValue(value as string);
        }
    }
}
