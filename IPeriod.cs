using System;

namespace CarLeasingViewer
{
    /// <summary>
    /// Период
    /// </summary>
    public interface IPeriod
    {
        /// <summary>
        /// Время начала
        /// </summary>
        DateTime DateStart { get; }

        /// <summary>
        /// Время окончания
        /// </summary>
        DateTime DateEnd { get; }
    }
}
