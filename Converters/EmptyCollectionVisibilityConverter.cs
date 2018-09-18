using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CarLeasingViewer.Converters
{
    /// <summary>
    /// Видимость элемента в зависимости от заполненности переданной коллекции
    /// </summary>
    public class EmptyCollectionVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Видимость при пустой коллекции (по умолчанию = Collapsed)
        /// </summary>
        public Visibility OnEmpty { get; set; } = Visibility.Collapsed;

        /// <summary>
        /// Видимость при полной коллекции (по умолчанию = Visible)
        /// </summary>
        public Visibility OnFilled { get; set; } = Visibility.Visible;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return OnEmpty;

            if(value is IEnumerable)
            {
                var ienum = ((IEnumerable)value).GetEnumerator();

                var hasElements = ienum.MoveNext();

                ienum.Reset();

                return hasElements ? OnFilled : OnEmpty;
            }

            return OnEmpty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
