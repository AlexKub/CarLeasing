using System;
using System.Globalization;
using System.Windows.Data;

namespace CarLeasingViewer.Converters
{

    public class ItemHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value + AppStyles.GridLineWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
