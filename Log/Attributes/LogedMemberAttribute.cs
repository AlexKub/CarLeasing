using System;

namespace CarLeasingViewer.Log
{
    /// <summary>
    /// Атрибут для логирования члена Класса
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class LogedMemberAttribute : Attribute
    {
        /// <summary>
        /// Логика конвертации значения
        /// </summary>
        /// <param name="memberValue">Значение</param>
        /// <returns>Строковое представление для логирования</returns>
        public virtual string ConvertValue(object memberValue)
        {
            if (memberValue == null)
                return "NULL";

            return memberValue.ToString();
        }

        /// <summary>
        /// Имя для логирования
        /// </summary>
        public string LogName { get; private set; }

        /// <summary>
        /// Метод приведения значения object к string у текущей цели аттрибута
        /// </summary>
        public LogedMemberAttribute(string logName)
        {
            LogName = logName;
        }
    }
}
