using System;

namespace CarLeasingViewer
{
    /// <summary>
    /// Флаги логеров
    /// </summary>
    public enum LogerFlag
    {
        /// <summary>
        /// Отсутствие флагов
        /// </summary>
        None,
        /// <summary>
        /// Логирование в Event log Windows
        /// </summary>
        WinLog,
        /// <summary>
        /// Логирование в файл
        /// </summary>
        LogFile
    }
}
