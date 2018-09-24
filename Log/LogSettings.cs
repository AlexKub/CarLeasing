using System;

namespace CarLeasingViewer
{
    [Serializable]
    public class LogSettings : MarshalByRefObject
    {
        /// <summary>
        /// Флаг Уровня логирования
        /// </summary>
        public int Log_Level { get; set; }

        /// <summary>
        /// Флаг набора логирования
        /// </summary>
        public string LogSetFlag { get; set; }

        /// <summary>
        /// Корневой каталог для файлового логирования. По умолчанию - пустая строка (текущий каталог приложения)
        /// </summary>
        public string FileLogingFolder { get; set; }

        public static LogSettings Default { get { return new LogSettings() { Log_Level = 3 }; } }
    }
}
