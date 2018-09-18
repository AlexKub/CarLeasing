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

        public static MonthBusiness GetRandomBusiness(int year = 2018)
        {
            return GetRandomBusiness(GetRandomMonth(year));
        }

        public static MonthBusiness GetRandomBusiness(Month month)
        {
            var mb = new MonthBusiness(Generate(month));

            mb.Month = month;

            return mb;
        }

        static IEnumerable<CarBusiness> Generate(Month month)
        {
            List<CarBusiness> values = new List<CarBusiness>();
            for (int i = 0; i < 100; i++)
            {
                values.Add(GenerateBusiness(i, month));
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

            for (int i = 0; i < count && end < dayCount; i++)
            {
                var b = new Business();
                b.Title = "bussy_" + i.ToString();
                start = m_rand.Next(end, dayCount);
                b.DateStart = new DateTime(month.Year, month.Index, start);
                end = m_rand.Next(start, dayCount);
                b.DateEnd = new DateTime(month.Year, month.Index, end);
                b.CurrentMonth = month;
                b.CarName = cb.Name;
                cb.Add(b);
                end++;
            }

            return cb;
        }

        
    }
}
