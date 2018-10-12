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

        //public static MonthBusiness GetRandomBusiness(Month start, Month end)
        //{
        //    var mb = new MonthBusiness(Generate(month));
        //
        //    mb.Month = month;
        //
        //    return mb;
        //}

        static IEnumerable<CarBusiness> Generate(Month month)
        {
            List<CarBusiness> values = new List<CarBusiness>();
            for (int i = 0; i < 100; i++)
            {
                values.Add(GenerateBusiness(i, month));
            }

            return values;
        }

        static IEnumerable<CarBusiness> Generate(Month start, Month end)
        {
            List<CarBusiness> values = new List<CarBusiness>();
            for (int i = 0; i < 100; i++)
            {
                values.Add(GenerateBusiness(i, start, end));
            }

            return values;
        }

        static CarBusiness GenerateBusiness(int index, Month month)
        {
            var cb = new CarBusiness();
            cb.Name = "Car_" + index.ToString();
            cb.Month = month;

            var count = m_rand.Next(1, 10);

            int start = 1;
            int end = 1;

            var dayCount = month.DayCount + 1;

            var blockedFlag = m_rand.Next(1, 25);
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
                b.Blocked = blockedFlag == 23; //магическое число 23
                cb.Add(b);
                end++;
            }

            return cb;
        }

        static CarBusiness GenerateBusiness(int index, Month start, Month end)
        {
            var cb = new CarBusiness();
            cb.Name = "Car_" + index.ToString();
            cb.Monthes = Month.GetMonthes(start, end);

            var count = m_rand.Next(1, 10);

            int startI = 1;
            var dayCount = Month.GetDaysCount(start, end);
            int endI = dayCount;

            for (int i = 0; i < count && endI < dayCount; i++)
            {
                var b = new Leasing();
                b.Title = "bussy_" + i.ToString();
                startI = m_rand.Next(startI, endI);
                b.DateStart = new DateTime(start.Year, start.Index, startI);
                endI = m_rand.Next(startI, dayCount);
                b.DateEnd = new DateTime(end.Year, end.Index, endI);
                //b.Monthes = cb.Monthes;
                b.CarName = cb.Name;
                b.Saler = "Saler_" + i.ToString();
                cb.Add(b);
                endI++;
            }

            return cb;
        }


    }
}
