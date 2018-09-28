using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace CarLeasingViewer.Converters
{
    public class MonthLeasingsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var monthLeasings = value as IReadOnlyList<MonthLeasing>;

            if (monthLeasings == null)
                return Enumerable.Empty<LeasingElementModel>();

            return monthLeasings.SelectMany(ml => ml.Leasings);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
