using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace CarLeasingViewer.Converters
{
    public class MonthesDaysCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var monthes = value as IEnumerable<MonthLeasing>;

            if (monthes == null)
                return 0;

            return monthes.Sum(m => m?.MonthHeader?.Month.DayCount);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
