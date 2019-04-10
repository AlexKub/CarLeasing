using System;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Сторнирование
    /// </summary>
    public class Storno : IPeriod
    {
        public DateTime DateStart { get; set; }

        public DateTime DateEnd { get; set; }

        public DateTime DocumentDate { get; set; }

        public int MonthCount { get;  set; }

        public string Comment { get; set; }

    }
}
