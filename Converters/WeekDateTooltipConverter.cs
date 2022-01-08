using System;
using System.Globalization;
using System.Windows.Data;

namespace CarLeasingViewer.Converters
{
    public class WeekDateTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((DateTime)value).ToString("dd MMMM yyyy", CultureInfo.CreateSpecificCulture("ru-RU"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
