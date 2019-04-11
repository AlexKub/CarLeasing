using System;

namespace CarLeasingViewer
{
    /// <summary>
    /// Расширение для ДатыВремени
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Подсчёт индекса текущего дня
        /// </summary>
        /// <param name="dateTime">Текущее время</param>
        /// <returns>Возвращает индекс для в системе</returns>
        public static int DayIndex(this DateTime dateTime)
        {
            if (dateTime.Equals(default(DateTime)))
                return 0;

            if (dateTime.Year == 1753)
                return 0;

            var index = 2000; //ранее 2000 нет истории, поэтому начинаем считать отсюда
            for (int i = index; i < dateTime.Year; i++)
            {
                index += DateTime.IsLeapYear(i) ? 366 : 365;
            }

            return index + dateTime.DayOfYear;
        }
    }
}
