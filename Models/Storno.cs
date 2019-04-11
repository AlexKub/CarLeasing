using System;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Сторнирование
    /// </summary>
    public class Storno : IPeriod
    {
        DateTime m_DateStart;
        public DateTime DateStart { get { return m_DateStart; } set { m_DateStart = value; DayIndexStart = value.DayIndex(); } }

        DateTime m_DateEnd;
        public DateTime DateEnd { get { return m_DateEnd; } set { m_DateEnd = value; DayIndexEnd = value.DayIndex(); } }

        public DateTime DocumentDate { get; set; }

        public int MonthCount { get;  set; }

        public string Comment { get; set; }

        public int DayIndexStart { get; private set; }

        public int DayIndexEnd { get; private set; }
    }
}
