using System;
using System.Collections.Generic;
using System.Runtime.Remoting;

namespace CarLeasingViewer
{
    /// <summary>
    /// Управление лоигрованием
    /// </summary>
    public class LogManager : MarshalByRefObject //наследуемся от маршалируемого объекта, т.к. класс передаются между AppDomain'ами в AdapterWrapper
    {
        const string winLogSource = "LogManager";


        public static string WinLogSource { get; set; } = "LogManager";
        /// <summary>
        /// Флаг для набора логирования по умолчанию
        /// </summary>
        internal const string DefaultLogSetFlag = "none";
        /// <summary>
        /// Флаг для набора логирования в файл
        /// </summary>
        internal const string LogSetFlag_File = "file";
        /// <summary>
        /// Флаг для набора логирования в файл
        /// </summary>
        internal const string LogSetFlag_UserFile = "ufile";
        /// <summary>
        /// Флаг для набора логирования в файл
        /// </summary>
        internal const string LogSetFlag_WinLog = "win";

        /// <summary>
        /// Набор логеров по умолчанию
        /// </summary>
        static readonly LogSet DefaultSet = GetDefaultLogSet();

        /// <summary>
        /// Возвращает или задаёт общие настройки логирования для текущего домена
        /// </summary>
        public static LogSettings Settings { get; set; }

        /// <summary>
        /// Наборы конечных логиров для логирования
        /// </summary>
        readonly static Dictionary<string, LogSet> m_logSets = new Dictionary<string, LogSet>();

        /// <summary>
        /// Текущий уровень логирования из файла настроек
        /// </summary>
        public static LogLevel LogLevel
        {
            get
            {
                var logLvl = LogLevel.Medium;
                try { logLvl = (LogLevel)LogManager.Settings.Log_Level; } catch { logLvl = LogLevel.Medium; }
                return logLvl;
            }
        }

        static LogManager()
        {
            ReadSettings();
        }

        /// <summary>
        /// Получение набора логирования по умолчанию (из файла настроек)
        /// </summary>
        /// <typeparam name="TClass">Логируемый тип</typeparam>
        /// <returns>Возвращает набор логирования для класса</returns>
        public static LogSet GetDefaultLogSet<TClass>()
        {
            return GetLogSet(typeof(TClass).FullName, null);
        }
        /// <summary>
        /// Получение набора логирования по умолчанию (из файла настроек)
        /// </summary>
        /// <param name="sourceName">Источник (логируемый тип)</param>
        /// <typeparam name="TClass">Логируемый тип</typeparam>
        /// <returns>Возвращает набор логирования для класса</returns>
        public static LogSet GetDefaultLogSet(string sourceName)
        {
            return GetLogSet(sourceName, null);
        }

        /// <summary>
        /// Получение специфического набора логирования
        /// </summary>
        /// <param name="flag">Флаг набора логирования</param>
        /// <typeparam name="TClass">Логируемый тип</typeparam>
        /// <returns>Возвращает набор логирования для класса</returns>
        public static LogSet GetLogSet<TClass>(string flag)
        {
            return GetLogSet(typeof(TClass).FullName, flag);
        }
        /// <summary>
        /// Получение специфического набора логирования
        /// </summary>
        /// <param name="sourceMarker">Маркер - источник логирования (для статических типов)</param>
        /// <param name="flag">Флаг набора (если известен)</param>
        /// <returns>Возвращает набор логирования для класса</returns>
        public static LogSet GetLogSet(string sourceMarker, string flag)
        {
            LogSet newSet = null;

            if (string.IsNullOrEmpty(sourceMarker))
                sourceMarker = "UnKnownSource";

            if (flag == null)
                if (Settings != null)
                    flag = Settings.LogSetFlag;

            if (!LogSetFlags.IsCorrectFlag(flag))
                flag = DefaultLogSetFlag;

            try
            {
                if (m_logSets.ContainsKey(flag))
                    newSet = m_logSets[flag].GetCopy();
                else
                {
                    newSet = Get_LogSetByFlag(flag);
                    m_logSets.Add(flag, newSet);
                }
            }
            catch (Exception ex)
            {
                WindowsLoger.LogError(winLogSource, "Возникло исключение при получении набора логирования для класса. Возвращён набор по умолчанию", ex,
                    new LogParameter("Логируемый тип", sourceMarker)
                    , new LogParameter("Флаг набора", flag == null ? "NULL" : flag));

                newSet = GetDefaultLogSet();
            }

            //ссылаемся на статическое всойство, т.к. предполагается,
            //что при первой загрузке кода в Домене, будут проставлены необходимые свойства
            //или впоследствии могут поменяться
            newSet.Settings = Settings;

            //добавляем имя класса при логировании сообщений
            newSet.AddParameter(new LogParameter("Тип", sourceMarker));
            newSet.AddParameter(new LogParameter("Пользователь", Environment.UserName));

            return newSet;
        }

        static void ReadSettings()
        {
            try
            {
                if (Settings == null)
                    Settings = LogSettings.Default;

                var flag = (Settings.LogSetFlag == null ? DefaultLogSetFlag : Settings.LogSetFlag).ToLower();

                if (!m_logSets.ContainsKey(DefaultLogSetFlag))
                    m_logSets.Add(DefaultLogSetFlag, DefaultSet);

                var settingsSet = Get_LogSetByFlag(flag);
                if (!m_logSets.ContainsKey(flag))
                    m_logSets.Add(flag, settingsSet);
            }
            catch (Exception ex)
            {
                WindowsLoger.LogError(winLogSource, "Возникло исключение при чтении файла настроек Менеджером логирования при инициализации. Возможен вал непредвиденных ошибок.", ex);
            }
        }

        static LogSet Get_LogSetByFlag(string setFlag)
        {
            if (string.IsNullOrEmpty(setFlag))
                setFlag = DefaultLogSetFlag;

            setFlag = setFlag.ToLower();

            LogSet set = new LogSet();
            switch (setFlag)
            {
                case LogSetFlag_File:
                    set.AddLoger(new FileLoger());
                    break;
                case LogSetFlag_UserFile:
                    set.AddLoger(new UserFileLoger());
                    break;
                case LogSetFlag_WinLog:
                    set.AddLoger(new WindowsLoger(WinLogSource));
                    break;
                //флаги наборов логирования добавлять сюда
                case DefaultLogSetFlag:
                default:
                    set = DefaultSet.GetCopy();
                    break;
            }

            return set;
        }

        /// <summary>
        /// Установка статических настроек через экземпляр
        /// </summary>
        /// <param name="settings">Устанавливаемые настройки</param>
        public void SetSettings(ObjectHandle settingsHandle)
        {
            /*
            метод используется для передачи в менеджер экземпляра настроек из другого домена
            используется в AdapterWrapper
            */

            var settings = settingsHandle.Unwrap() as LogSettings;

            if (settings != null)
                Settings = settings;
        }

        static LogSet GetDefaultLogSet()
        {
            try
            {
                var set = new LogSet();

                set.AddLoger(new WindowsLoger(WinLogSource));
                return set;
            }
            catch (Exception ex)
            {
                WindowsLoger.LogError(winLogSource, "Возникло исключение при генерации набора логирования по умолчанию. Возвращена пустая ссылка на Набор. Возможен вал непредвиденных ошибок.", ex);

                return null;
            }
        }

        /// <summary>
        /// Флаги извествных наборов логирования
        /// </summary>
        public class LogSetFlags
        {
            /*
            делаем через пропертя, чтобы избежать inline'a const'ант при компиляции, в случае прямого использования
            */

            /// <summary>
            /// Флаг для набора логирования по умолчанию
            /// </summary>
            public static string Default { get { return LogManager.DefaultLogSetFlag; } }
            /// <summary>
            /// Флаг для набора логирования в файл
            /// </summary>
            public static string File { get { return LogManager.LogSetFlag_File; } }
            /// <summary>
            /// Флаг для набора логирования в файл
            /// </summary>
            public static string UserFile { get { return LogManager.LogSetFlag_UserFile; } }
            /// <summary>
            /// Флаг для набора логирования в файл
            /// </summary>
            public static string WinLog { get { return LogManager.LogSetFlag_WinLog; } }

            /// <summary>
            /// Проверка на наличие подобного флага среди указанных
            /// </summary>
            /// <param name="flag">Флаг для проверки</param>
            /// <returns>Возвращает флаг соответствия переданного флага одному из указанных</returns>
            public static bool IsCorrectFlag(string flag)
            {
                if (string.IsNullOrEmpty(flag))
                    return false;

                flag = flag.ToLower();

                switch (flag)
                {
                    case LogManager.DefaultLogSetFlag:
                    case LogManager.LogSetFlag_File:
                    case LogManager.LogSetFlag_UserFile:
                    case LogManager.LogSetFlag_WinLog:
                        return true;
                    default:
                        return false;
                }
            }
        }
    }
}
