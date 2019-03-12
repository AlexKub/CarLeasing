using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;

namespace CarLeasingViewer
{
    public static class Randomizer
    {
        static System.Random m_rand = new System.Random();

        public static IEnumerable<Month> GenerateMonthes(int year)
        {
            var months = new List<Month>();

            for (int i = 1; i < 13; i++)
            {
                months.Add(new Month(year, (Monthes)i));
            }

            return months;
        }

        public static Month GetRandomMonth(int year)
        {
            var index = m_rand.Next(1, 13);

            return new Month(year, (Monthes)index);
        }

        public static MonthBusiness GetRandomBusiness(Month month)
        {
            var mb = new MonthBusiness(Generate(month));

            mb.Month = month;

            return mb;
        }

        public static MonthBusiness GetRandomBusiness(Month start, Month end)
        {
            var monthes = Month.GetMonthes(start, end);

            var mb = new MonthBusiness(Generate(start, end));
            mb.Monthes = monthes;

            return mb;
        }

        static IEnumerable<ItemInfo> Generate(Month month)
        {
            List<ItemInfo> values = new List<ItemInfo>();
            for (int i = 0; i < 100; i++)
            {
                values.Add(GenerateBusiness(i, month));
            }

            return values;
        }

        static IEnumerable<ItemInfo> Generate(Month start, Month end)
        {
            List<ItemInfo> values = new List<ItemInfo>();
            for (int i = 0; i < 100; i++)
            {
                values.Add(GenerateBusiness(i, start, end));
            }

            return values;
        }

        static ItemInfo GenerateBusiness(int index, Month month)
        {
            var cb = new ItemInfo();
            cb.Name = "Car_" + index.ToString();
            cb.Month = month;

            var count = m_rand.Next(1, 10);

            int start = 1;
            int end = 1;

            var dayCount = month.DayCount + 1;

            var blockedFlag = m_rand.Next(1, 25);
            var addBlocked = App.SearchSettings.IncludeBlocked;
            for (int i = 0; i < count && end < dayCount; i++)
            {
                blockedFlag = m_rand.Next(1, 25);
                var b = new Leasing();
                b.Title = "bussy_" + i.ToString();
                start = m_rand.Next(end, dayCount);
                b.DateStart = new DateTime(month.Year, month.Index, start);
                end = m_rand.Next(start, dayCount);
                b.DateEnd = new DateTime(month.Year, month.Index, end);
                b.CurrentMonth = month;
                b.CarName = cb.Name;
                b.Saler = "Saler_" + i.ToString();

                if (addBlocked)
                    b.Blocked = blockedFlag == 23; //магическое число 23 (просто случайное число в пределах диапазона)

                cb.Add(b);
                end++;
            }

            return cb;
        }

        static ItemInfo GenerateBusiness(int index, Month start, Month end)
        {
            var cb = new ItemInfo();
            cb.Name = "Car_" + index.ToString();
            //start = new Month(start.Year, m_rand.Next(start.Index, end.Index + 1));
            //end = new Month(end.Year, m_rand.Next(start.Index, end.Index + 1));
            cb.Monthes = Month.GetMonthes(start, end);

            var count = m_rand.Next(1, 10);

            int startDayI = 1;
            var dayCount = Month.GetDaysCount(start, end);
            int endDayI = end.DayCount;
            int monthIndex = 0;

            var blockedFlag = m_rand.Next(1, 25);
            var addBlocked = App.SearchSettings.IncludeBlocked;
            for (int mi = start.Index; mi <= end.Index; mi++)
            {
                monthIndex = mi;

                for (int i = 0; i < count && endDayI <= end.DayCount; i++)
                {
                    var b = new Leasing();
                    b.Title = "bussy_" + i.ToString();
                    startDayI = m_rand.Next(startDayI, endDayI);
                    b.DateStart = new DateTime(start.Year, monthIndex, startDayI);
                    endDayI = m_rand.Next(startDayI, dayCount);
                    //b.DateEnd = new DateTime(end.Year, end.Index, end.DayCount <= endI ? endI : end);
                    b.Monthes = cb.Monthes;
                    b.CarName = cb.Name;
                    b.Saler = "Saler_" + i.ToString();

                    if (addBlocked)
                        b.Blocked = blockedFlag == 23; //магическое число 23 (просто случайное число в пределах диапазона)
                    cb.Add(b);
                    endDayI++;
                }
            }

            return cb;
        }


    }
}
