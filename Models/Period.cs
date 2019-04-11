using System;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Период
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class Period : IPeriod
    {
        /// <summary>
        /// Дата начала
        /// </summary>
        public DateTime DateStart { get; private set; }
        /// <summary>
        /// Дата окончания
        /// </summary>
        public DateTime DateEnd { get; private set; }
        /// <summary>
        /// Количество месяцев
        /// </summary>
        public int MonthCount { get; private set; }
        /// <summary>
        /// Индекс начального дня
        /// </summary>
        public int DayIndexStart { get; private set; }
        /// <summary>
        /// Индекс конечного дня
        /// </summary>
        public int DayIndexEnd { get; private set; }

        public Period(DateTime start, DateTime end)
        {
            DateStart = start;
            DateEnd = end;
            DayIndexStart = start.DayIndex();
            DayIndexEnd = end.DayIndex();
            MonthCount = this.CalculateMonthCount();
        }

        string DebugDisplay()
        {
            return $"{DateStart.ToString()} - {DateEnd.ToString()}";
        }
    }
}
