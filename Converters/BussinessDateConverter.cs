using CarLeasingViewer.Models;
using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace CarLeasingViewer.Converters
{
    public class BussinessDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "NULL VALUE";

            var b = value as Business;

            if (b == null)
                return "NULL BUSINESS";

            StringBuilder sb = new StringBuilder();

            var action = "";
            switch (b.Type)
            {
                default: action = "в прокате"; break;
            }

            //<действие> c XX по ХХ <месяц>
            sb.Append(action).Append(" c ");

            if (b.MonthCount < 2)
                sb.Append(b.DateStart.Day.ToString()).Append(" по ").Append(b.DateEnd.Day.ToString()).Append(" ").Append(b.DateStart.GetMonthName() ?? string.Empty);
            else
                sb.Append(b.DateStart.Day.ToString()).Append(" ").Append(b.DateStart.GetMonthName() ?? string.Empty).Append(" по ")
                    .Append(b.DateEnd.Day.ToString()).Append(" ").Append(b.DateEnd.GetMonthName() ?? string.Empty);

            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
