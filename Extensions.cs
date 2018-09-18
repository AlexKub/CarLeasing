using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;

namespace CarLeasingViewer
{
    public static class Extensions
    {
        public static int Max(this IList<int> list)
        {
            if (list.Count == 0)
                return 0;

            int max = list[0];
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i] > max)
                    max = list[i];
            }

            return max;
        }

        public static int Min(this IList<int> list)
        {
            if (list.Count == 0)
                return 0;

            int min = list[0];
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i] < min)
                    min = list[i];
            }

            return min;
        }

        public static Month GetMonth(this DateTime date)
        {
            return new Month(date);
        }

        public static string GetMonthName(this DateTime date)
        {
            return Month.GetRussianName((Monthes)date.Month);
        }

        public static int IndexOf<T>(this IEnumerable<T> collection, T value, Func<T, bool> compare = null)
        {
            var index = -1;

            if (value == null)
                return index;

            if (compare == null)
                compare = (o) => o.Equals(value);

            foreach (var item in collection)
            {
                index++;

                if (item != null)
                    if (compare(item))
                        return index;
            }

            return index;
        }

    }
}
