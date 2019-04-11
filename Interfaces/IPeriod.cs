using System;

namespace CarLeasingViewer
{
    /// <summary>
    /// Период
    /// </summary>
    public interface IPeriod
    {
        /// <summary>
        /// Индекс начального дня
        /// </summary>
        int DayIndexStart { get; }
        /// <summary>
        /// Индекс конечного дня
        /// </summary>
        int DayIndexEnd { get; }
        /// <summary>
        /// Время начала
        /// </summary>
        DateTime DateStart { get; }
        /// <summary>
        /// Время окончания
        /// </summary>
        DateTime DateEnd { get; }
        /// <summary>
        /// Количество месяцев
        /// </summary>
        int MonthCount { get; }
    }
}
