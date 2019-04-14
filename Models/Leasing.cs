using System;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Единица аренды авто
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class Leasing : IPeriod
    {
        public string Title { get; set; }

        DateTime m_dateStart;
        public DateTime DateStart
        {
            get { return m_dateStart; }
            set
            {
                m_dateStart = value;
                DayIndexStart = value.DayIndex();
                MonthCount = this.CalculateMonthCount();
            }
        }
        DateTime m_DateEnd;
        public DateTime DateEnd
        {
            get { return m_DateEnd; }
            set
            {
                m_DateEnd = value;
                DayIndexEnd = value.DayIndex();
                MonthCount = this.CalculateMonthCount();
            }
        }

        public string Comment { get; set; }

        public BusinessType Type { get; set; }

        public Month CurrentMonth { get; set; }

        public Month[] Monthes { get; set; }

        public string Width { get; set; }

        public string CarName { get; set; }

        public string Saler { get; set; }

        public string DocNumber { get; set; }

        public bool Blocked { get; set; }

        public bool Include(DateTime date)
        {
            return (DateStart <= date) && (date <= DateEnd);
        }

        public bool Cross(DateTime start, DateTime end)
        {
            return !((DateStart > end) || (DateEnd < start));
        }

        public int MonthCount { get; private set; }

        public int DayIndexStart { get; private set; }

        public int DayIndexEnd { get; private set; }

        public decimal DayCount { get; set; }

        string DebugDisplay()
        {
            return string.IsNullOrEmpty(Title) ? "EMPTY TITLE" : Title;
        }
    }

    public enum BusinessType
    {
        UnKnown,
        Leasing,
        PTS
    }
}
