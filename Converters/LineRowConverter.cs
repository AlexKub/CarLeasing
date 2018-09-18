using CarLeasingViewer.Controls;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Shapes;

namespace CarLeasingViewer.Converters
{
    public class LineRowConverter : IValueConverter
    {
        private int m_lineIndex = 0;
        Line m_curentLine;

        public LineGrid ParentCollection { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0d;

            if (ParentCollection == null)
                return 0d;

            var line = value as Line;
            if (line == null)
                throw new ArgumentNullException("Переданный тип значения отличается от Line");

            if (line != m_curentLine)
            {
                m_curentLine = line;
                m_lineIndex++;
            }

            return ParentCollection.GetRowHeight(m_lineIndex);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            m_lineIndex = 0;
        }
    }
}
