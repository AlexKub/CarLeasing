using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace CarLeasingViewer.Converters
{
    public class ColumnWidthConverter : IValueConverter
    {
        public int ColumnIndex { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var def = value as ColumnDefinitionCollection;

            if (def == null)
                return 0d;

            if (ColumnIndex < def.Count)
            {
                return def[ColumnIndex].ActualWidth;
            }
            else
                return 0d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
