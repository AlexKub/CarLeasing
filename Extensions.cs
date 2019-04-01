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

        /// <summary>
        /// Получение месяца из даты
        /// </summary>
        /// <param name="date">Дата</param>
        /// <returns>Возвращает соответствующий месяц</returns>
        public static Month GetMonth(this DateTime date)
        {
            return new Month(date);
        }

        /// <summary>
        /// Получение русского имени месяца для даты
        /// </summary>
        /// <param name="date">Дата</param>
        /// <returns>Возвращает соответствующее имя месяца</returns>
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

        /// <summary>
        /// Проверка, что в коллекции есть хотя бы один элемент или она проинициализирована
        /// </summary>
        /// <typeparam name="T">Тип элементов в коллекции</typeparam>
        /// <param name="collection">Коллекция элементов (допустим null)</param>
        /// <returns>Возвращает флаг, что в коллекции присутсвует хотя бы один элемент</returns>
        public static bool IsEmpty<T>(this IEnumerable<T> collection)
        {
            if (collection == null)
                return true;

            foreach (var item in collection)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверка калличия значений по ссылке на указанную цену
        /// </summary>
        /// <param name="price">Цена</param>
        /// <returns>Возвращает наличие значений по указанной ссылке</returns>
        public static bool IsNullOrEmpty(this CarPriceList price)
        {
            if (price == null)
                return false;

            return price.IsEmpty;
        }

        /// <summary>
        /// Получение видимого представления поиска по БД
        /// </summary>
        /// <param name="type">Тип поиска по БД</param>
        /// <returns>Возвращает видимое представление</returns>
        public static string GetDescription(this DBSearchType type)
        {
            return DBSearchTypeHelper.GetDescription(type);
        }

        public static string GetSqlDate(this DateTime date)
        {
            return date.Year.ToString() + date.Month.ToString("00") + date.Day.ToString("00");
        }

        /// <summary>
        /// НЕ пустое значение строки для логирования
        /// </summary>
        /// <param name="value">Текущее значение строки</param>
        /// <returns>Возвращает текущее значение или флаги-заместители, если оно пустое</returns>
        public static string LogValue(this string value)
        {
            return value == null
                ? "NULL"
                : string.IsNullOrWhiteSpace(value)
                    ? "EMPTY"
                    : value;
        }
    }
}
