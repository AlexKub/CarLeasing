using System;
using System.Reflection;

namespace CarLeasingViewer.Tests
{
    static class Utils
    {
        /// <summary>
        /// Получение значения приватного поля
        /// </summary>
        /// <typeparam name="TValue">Тип получаемого значения</typeparam>
        /// <typeparam name="TInstance">Тип экземпляра, на котором получаем значение</typeparam>
        /// <param name="fieldName">Имя поля</param>
        /// <param name="instance">Экземпляр. Для статических полей - null</param>
        /// <param name="flags">Флаги поиска</param>
        /// <returns>Возвращает значение поля в экземпляре по имени</returns>
        public static TValue GetFiledValue<TValue, TInstance>(string fieldName, BindingFlags flags, TInstance instance) where TInstance : class
        {
            Type t = typeof(TInstance);

            var f = t.GetField(fieldName, flags);

            if (f == null)
                throw new ArgumentNullException($"Поле '{fieldName}' не найдено в типе '{t.Name}'");

            return (TValue)f.GetValue(instance);
        }

        /// <summary>
        /// Получение значения приватного поля экземпляра
        /// </summary>
        /// <typeparam name="TValue">Тип получаемого значения</typeparam>
        /// <typeparam name="TInstance">Тип экземпляра, на котором получаем значение</typeparam>
        /// <param name="fieldName">Имя поля</param>
        /// <param name="instance">Экземпляр. Для статических полей - null</param>
        /// <param name="flags">Флаги поиска</param>
        /// <returns>Возвращает значение поля в экземпляре по имени</returns>
        public static TValue GetInstanceFiledValue<TValue, TInstance>(string fieldName, TInstance instance) where TInstance : class
        {
            return GetFiledValue<TValue, TInstance>(fieldName, BindingFlags.NonPublic | BindingFlags.Instance, instance);
        }

        /// <summary>
        /// Получение значения приватного статического поля
        /// </summary>
        /// <typeparam name="TValue">Тип получаемого значения</typeparam>
        /// <typeparam name="TInstance">Тип экземпляра, на котором получаем значение</typeparam>
        /// <param name="fieldName">Имя поля</param>
        /// <param name="instance">Экземпляр. Для статических полей - null</param>
        /// <param name="flags">Флаги поиска</param>
        /// <returns>Возвращает значение поля в экземпляре по имени</returns>
        public static TValue GetStaticFiledValue<TValue, TInstance>(string fieldName) where TInstance : class
        {
            return GetFiledValue<TValue, TInstance>(fieldName, BindingFlags.NonPublic | BindingFlags.Static, null);
        }
    }
}
