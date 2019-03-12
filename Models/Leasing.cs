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

        public DateTime DateStart { get; set; }

        public DateTime DateEnd { get; set; }

        public string Comment { get; set; }

        public BusinessType Type { get; set; }

        public Month CurrentMonth { get; set; }

        public Month[] Monthes { get; set; }

        public string Width { get; set; }

        public string CarName { get; set; }

        public string Saler { get; set; }

        public bool Blocked { get; set; }

        public bool Include(DateTime date)
        {
            return (DateStart <= date) && (date <= DateEnd);
        }

        public bool Cross(DateTime start, DateTime end)
        {
            return !((DateStart > end) || (DateEnd < start));
        }

        public int MonthCount
        {
            get
            {
                //если есть разница в годе (взята в одном году,а возвращается в другом)
                if (DateStart.Year != DateEnd.Year)
                {
                    var monthCount = DateEnd.Month; //количество месяцев в крайнем году

                    int yearCount = DateEnd.Year - DateStart.Year;
                    int curentYear = DateStart.Year;
                    for (int y = 0; y < yearCount; y++)
                    {
                        if (curentYear == DateStart.Year) //если сейчас начальный год
                            monthCount += ((12 - DateStart.Month) + 1); //добавляем количество месяцев в текущем году, за вычетом прошедших
                        else
                            monthCount += 12; //добавляем все 12 месяцев, если мышина взята на период более 2 лет О_о

                        curentYear++;
                    }

                    return monthCount;
                }
                else
                    return (DateEnd.Month - DateStart.Month) + 1;
            }
        }

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
