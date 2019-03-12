using System;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Информация о ремонте авто
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class MaintenanceInfo : IPeriod
    {
        /// <summary>
        /// Дата начала
        /// </summary>
        public DateTime DateStart { get; set; }

        /// <summary>
        /// Дата окончания
        /// </summary>
        public DateTime DateEnd { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        string DebugDisplay()
        {
            return $"{DateStart.ToString()} - {DateEnd.ToString()}";
        }
    }
}
