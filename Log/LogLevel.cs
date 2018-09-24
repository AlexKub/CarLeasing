

namespace CarLeasingViewer
{
    /// <summary>
    /// Уровень логирования
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Логируются только ошибки
        /// </summary>
        Low = 0,
        /// <summary>
        /// Логируются только предупреждения и ошибки
        /// </summary>
        Medium = 1,
        /// <summary>
        /// Логируются все сообщения, кроме отладочных
        /// </summary>
        Hight = 2,
        /// <summary>
        /// Логируются все сообщения, включая отладочные
        /// </summary>
        Debug = 3,
    }
}
