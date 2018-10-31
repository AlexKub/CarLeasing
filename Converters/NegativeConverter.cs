using System;
using System.Globalization;
using System.Windows.Data;

namespace CarLeasingViewer.Converters
{
    public class NegativeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            switch (value.GetType().Name)
            {
                case nameof(Int32):
                    return ((int)value * -1);
                case nameof(Double):
                    return ((double)value * -1d);
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
