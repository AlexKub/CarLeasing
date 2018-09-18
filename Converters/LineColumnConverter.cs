using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Shapes;

namespace CarLeasingViewer.Converters
{
    public class LineColumnConverter : DependencyObject, IValueConverter
    {
        private int m_columnIndex = 0;
        Line m_curentLine;
        /// <summary>
        /// Смещение слева от первой колонки с именами авто
        /// </summary>
        double m_columnOffset;
        /// <summary>
        /// Ширина колонки с индексом дня/именем дня недели
        /// </summary>
        double m_columnWidth;

        public static DependencyProperty dp_FirstColumnOffset = DependencyProperty.Register(nameof(FirstColumnOffset), typeof(double), typeof(LineColumnConverter), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(double)
            ,
            PropertyChangedCallback = (s, e) =>
            {
                (s as LineColumnConverter).m_columnOffset = (double)e.NewValue + 2; // ~2 пикселя добавляется от DockPanel
            }
        });
        public double FirstColumnOffset { get { return (double)GetValue(dp_FirstColumnOffset); } set { SetValue(dp_FirstColumnOffset, value); } }

        public static DependencyProperty dp_ColumnWidth = DependencyProperty.Register(nameof(ColumnWidth), typeof(double), typeof(LineColumnConverter), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(double)
        ,
            PropertyChangedCallback = (s, e) => { (s as LineColumnConverter).m_columnWidth = (double)e.NewValue; }
        });
        public double ColumnWidth { get { return (double)GetValue(dp_ColumnWidth); } set { SetValue(dp_ColumnWidth, value); } }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0d;

            var line = value as Line;
            if (line == null)
                throw new ArgumentNullException("Переданный тип значение отличается от Line");

            if (line != m_curentLine)
            {
                m_curentLine = line;
                m_columnIndex++;
            }

            return m_columnOffset + (m_columnIndex - 1) * (m_columnWidth + 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            m_columnIndex = 0;
            m_curentLine = null;
        }
    }
}
