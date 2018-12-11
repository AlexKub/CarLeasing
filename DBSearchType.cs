using System;
using System.Collections.Generic;
using System.Linq;

namespace CarLeasingViewer
{
    /// <summary>
    /// Типы поиска по БД
    /// </summary>
    public enum DBSearchType
    {
        /// <summary>
        /// Всё
        /// </summary>
        All,
        /// <summary>
        /// Архивные данные
        /// </summary>
        Old,
        /// <summary>
        /// Акутальные
        /// </summary>
        Curent
    }

    /// <summary>
    /// Вспомогательные методы для Типов поиска по БД
    /// </summary>
    public static class DBSearchTypeHelper
    {
        /*
         * сопоставление значений перечисления с видимыми описаниями для View
         */

        /// <summary>
        /// Описание по умолчанию
        /// </summary>
        public static readonly string DefaultDescription = "не определено";

        /// <summary>
        /// Значение по умолчанию
        /// </summary>
        public static readonly DBSearchType DefaultValue = DBSearchType.All;

        /// <summary>
        /// Набор сопоставления
        /// </summary>
        static List<Tuple<DBSearchType, string>> m_descriptions = new List<Tuple<DBSearchType, string>>()
        {
            new Tuple<DBSearchType, string>(DBSearchType.All, "Все")
            ,new Tuple<DBSearchType, string>(DBSearchType.Old, "Учтённые")
            ,new Tuple<DBSearchType, string>(DBSearchType.Curent, "Активные")
        };

        /// <summary>
        /// Количество имеющихся описаний
        /// </summary>
        public static int DescriptionsCount { get { return m_descriptions.Count; } }

        /// <summary>
        /// Получение видимого представления поиска по БД
        /// </summary>
        /// <param name="type">Тип поиска по БД</param>
        /// <returns>Возвращает видимое представление</returns>
        public static string GetDescription(DBSearchType type)
        {
            //строковое представление типа поиска для View

            var pair = m_descriptions.FirstOrDefault(t => t.Item1 == type);

            return pair == null ? DefaultDescription : pair.Item2;
        }

        /// <summary>
        /// Получение значения Типа поиска по его видимому представлению
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public static DBSearchType GetValue(string description)
        {
            if (description == null)
                return DefaultValue;

            var pair = m_descriptions.FirstOrDefault(t => description.Equals(t.Item2));

            return pair == null ? DefaultValue : pair.Item1;
        }

        /// <summary>
        /// Получение всех описаний типов поиска по БД
        /// </summary>
        /// <param name="type">Тип поиска по БД</param>
        /// <returns>Возвращает все типы поиска по БД для View</returns>
        public static string[] GetAllDescriptions()
        {
            var descriptions = new List<string>();
            foreach (var item in Enum.GetValues(typeof(DBSearchType)))
            {
                descriptions.Add(GetDescription((DBSearchType)item));
            }

            return descriptions.ToArray();
        }
    }
}
