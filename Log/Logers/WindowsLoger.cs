using System;
using System.Diagnostics;
using System.Text;

namespace CarLeasingViewer
{
    /// <summary>
    /// Логирование сообщений в журнал Windows
    /// </summary>
    public class WindowsLoger : ILoger
    {
        string _source;

        const string DefaultLogName = "Application";

        public event Action<ILoger<ILogParameter>> LogerError;

        /// <summary>
        /// Максимальная длина сообщения для журнала логирования Windows
        /// </summary>
        public const int MaxMessageLength = 32766;

        /// <summary>
        /// Возвращает или задаёт Наименование источника сообщений для Журанала Windows
        /// </summary>
        public string Source
        {
            get { return _source; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Для WindowsLoger передана пустая ссылка на имя источника. Логирование в Windows без источника не предусмотрено.");

                _source = value;
            }
        }

        public string LogName { get; set; } = DefaultLogName;

        /// <summary>
        /// Создание нового экземпляра Логера
        /// </summary>
        /// <param name="source"></param>
        public WindowsLoger(string source)
        {
            if (source == null)
                throw new ArgumentNullException("Для WindowsLoger не передано имя источника. Логирование в Windows без источника не предусмотрено.");

            _source = source;
        }

        /// <summary>
        /// Запись информации об исключении в журнал Application
        /// </summary>
        /// <param name="message">Описание</param>
        /// <param name="ex">Экземпляр возникшего исключения</param>
        /// <param name="parameters">Параметры сообщения</param>
        public void LogError(string message, Exception ex, params ILogParameter[] parameters)
        {
            LogError(message, ex, DefaultLogName, parameters);
        }
        /// <summary>
        /// Запись информации об исключении в журнал логирования Windows
        /// </summary>
        /// <param name="message">Описание</param>
        /// <param name="ex">Экземпляр возникшего исключения</param>
        /// <param name="logName">Имя журнала логирования</param>
        /// <param name="parameters">Параметры сообщения</param>
        public void LogError(string message, Exception ex, string logName, params ILogParameter[] parameters)
        {
            LogError(message, ex, DefaultLogName, EventLogEntryType.Error, parameters);
        }
        /// <summary>
        /// Запись информации об исключении в журнал логирования Windows
        /// </summary>
        /// <param name="message">Описание</param>
        /// <param name="ex">Экземпляр возникшего исключения</param>
        /// <param name="logName">Имя журнала логирования</param>
        /// <param name="eventType">Тип события в Журнале</param>
        /// <param name="parameters">Параметры сообщения</param>
        public void LogError(string message, Exception ex, string logName, EventLogEntryType eventType, params ILogParameter[] parameters)
        {
            var logMessage = LogMessageBuilder.BuildMessage(message, ex, parameters: parameters);

            WriteLogMessage(logName, logMessage, eventType);
        }

        /// <summary>
        /// Запись сообщения в журнал логирования Application
        /// </summary>
        /// <param name="message">Сообщение для записи</param>
        /// <param name="messageType">Тип сообщения</param>
        /// <param name="parameters">Параметры сообщения</param>
        public void LogMessage(string message, params ILogParameter[] parameters)
        {
            LogMessage(message, GetWindowsLogType(MessageType.Info), parameters);
        }

        /// <summary>
        /// Запись сообщения в журнал логирования Application
        /// </summary>
        /// <param name="message">Сообщение для записи</param>
        /// <param name="messageType">Тип сообщения</param>
        /// <param name="parameters">Параметры сообщения</param>
        public void LogMessage(string message, EventLogEntryType messageType, params ILogParameter[] parameters)
        {
            LogMessage(message, messageType, DefaultLogName, parameters);
        }
        /// <summary>
        /// Запись сообщения в журнал логирования Application
        /// </summary>
        /// <param name="message">Сообщение для записи</param>
        /// <param name="messageType">Тип сообщения</param>
        /// <param name="parameters">Параметры сообщения</param>
        public void LogMessage(string message, MessageType messageType, params ILogParameter[] parameters)
        {
            LogMessage(message, GetWindowsLogType(messageType), DefaultLogName, parameters);
        }
        /// <summary>
        /// Запись сообщения в журнал логирования Windows
        /// </summary>
        /// <param name="message">Сообщение для записи</param>
        /// <param name="messageType">Тип сообщения</param>
        /// <param name="logName">Имя жрунала логирования</param>
        /// <param name="parameters">Параметры сообщения</param>
        public void LogMessage(string message, EventLogEntryType messageType, string logName, params ILogParameter[] parameters)
        {
            var logMessage = LogMessageBuilder.BuildMessage(message, parameters: parameters);

            WriteLogMessage(logName, logMessage, messageType);
        }

        #region public static methods

        /// <summary>
        /// Запись информации об исключении в журнал Application
        /// </summary>
        /// <param name="source">Наименование источника (любое имя для идентификации источника в Журнале)</param>
        /// <param name="message">Описание</param>
        /// <param name="ex">Экземпляр возникшего исключения</param>
        /// <param name="parameters">Параметры сообщения</param>
        public static void LogError(string source, string message, Exception ex, params ILogParameter[] parameters)
        {
            LogError(source, message, ex, DefaultLogName, parameters);
        }
        /// <summary>
        /// Запись информации об исключении в журнал логирования Windows
        /// </summary>
        /// <param name="source">Наименование источника (любое имя для идентификации источника в Журнале)</param>
        /// <param name="message">Описание</param>
        /// <param name="ex">Экземпляр возникшего исключения</param>
        /// <param name="logName">Имя журнала логирования</param>
        /// <param name="parameters">Параметры сообщения</param>
        public static void LogError(string source, string message, Exception ex, string logName, params ILogParameter[] parameters)
        {
            var logMessage = message + " : " + ex.ToString();

            LogMessage(source, logMessage, EventLogEntryType.Error, logName, parameters: parameters);
        }

        #region LogMessage

        /// <summary>
        /// Запись информационного сообщения журнал логирования Windows
        /// </summary>
        /// <param name="source">Наименование источника (обязательно) (любое имя для идентификации источника в Журнале)</param>
        /// <param name="message">Описание</param>
        /// <exception cref="ArgumentNullException">Указание источника обязательно</exception>
        public static void LogMessage(string source, string message, params ILogParameter[] parameters)
        {
            LogMessage(source, message, EventLogEntryType.Information, DefaultLogName, parameters);
        }
        /// <summary>
        /// Запись сообщения в журнал логирования Windows указанного типа
        /// </summary>
        /// <param name="source">Наименование источника (обязательно) (любое имя для идентификации источника в Журнале)</param>
        /// <param name="message">Описание</param>
        /// <param name="messageType">Тип сообщения</param>
        /// <exception cref="ArgumentNullException">Указание источника обязательно</exception>
        public static void LogMessage(string source, string message, EventLogEntryType messageType, params ILogParameter[] parameters)
        {
            LogMessage(source, message, messageType, DefaultLogName, parameters);
        }
        /// <summary>
        /// Запись сообщения в журнал логирования Windows указанного типа
        /// </summary>
        /// <param name="source">Наименование источника (обязательно) (любое имя для идентификации источника в Журнале)</param>
        /// <param name="message">Описание</param>
        /// <param name="messageType">Тип сообщения</param>
        /// <exception cref="ArgumentNullException">Указание источника обязательно</exception>
        public static void LogMessage(string source, string message, MessageType messageType, params ILogParameter[] parameters)
        {
            LogMessage(source, message, GetWindowsLogType(messageType), DefaultLogName, parameters);
        }
        /// <summary>
        /// Запись информации об исключении в журнал логирования Windows
        /// </summary>
        /// <param name="source">Наименование источника (обязательно) (любое имя для идентификации источника в Журнале)</param>
        /// <param name="message">Описание</param>
        /// <param name="messageType">Тип сообщения</param>
        /// <exception cref="ArgumentNullException">Указание источника обязательно</exception>
        public static void LogMessage(string source, string message, EventLogEntryType messageType, string logName, params ILogParameter[] parameters)
        {
            var loger = new WindowsLoger(source);

            loger.LogMessage(message, messageType, parameters: parameters);
        }
        /// <summary>
        /// Запись информации об исключении в журнал логирования Windows
        /// </summary>
        /// <param name="source">Наименование источника (обязательно) (любое имя для идентификации источника в Журнале)</param>
        /// <param name="message">Описание</param>
        /// <param name="messageType">Тип сообщения</param>
        /// <exception cref="ArgumentNullException">Указание источника обязательно</exception>
        public static void LogMessage(string source, string message, MessageType messageType, string logName, params ILogParameter[] parameters)
        {
            LogMessage(source, message, GetWindowsLogType(messageType), DefaultLogName, parameters);
        }

        #endregion

        public static void WriteToLog(EventLog log, string message, EventLogEntryType messageType, params ILogParameter[] parameters)
        {
            var logMessage = LogMessageBuilder.BuildMessage(message, parameters: parameters);

            try
            {
                log.WriteEntry(logMessage, messageType);
            }
            catch
            {
            }
        }
        public static void WriteToLog(EventLog log, string message, Exception ex, params ILogParameter[] parameters)
        {
            WriteToLog(log, message, ex, EventLogEntryType.Error, parameters);
        }
        public static void WriteToLog(EventLog log, string message, Exception ex, EventLogEntryType messageType, params ILogParameter[] parameters)
        {
            var logMessage = LogMessageBuilder.BuildMessage(message, ex, parameters: parameters);

            try
            {
                log.WriteEntry(logMessage, messageType);
            }
            catch { }
        }

        public static EventLogEntryType GetWindowsLogType(MessageType messageType)
        {
            var logEventType = EventLogEntryType.Information;

            switch (messageType)
            {
                case MessageType.Warning:
                    logEventType = EventLogEntryType.Warning;
                    break;
                case MessageType.Error:
                    logEventType = EventLogEntryType.Error;
                    break;
                default:
                    break;
            }

            return logEventType;
        }

        public static MessageType GetECRLogType(EventLogEntryType messageType)
        {
            switch (messageType)
            {
                case EventLogEntryType.Warning:
                    return MessageType.Warning;
                case EventLogEntryType.Error:
                    return MessageType.Error;
                case EventLogEntryType.Information:
                    return MessageType.Info;
                default:
                    return MessageType.Info;
            }
        }

        #endregion

        #region ILoger

        void ILoger<ILogParameter>.Log(string message, params ILogParameter[] parameters)
        {
            LogMessage(message, EventLogEntryType.Information, parameters: parameters);
        }

        void ILoger<ILogParameter>.Log(string message, MessageType type, params ILogParameter[] parameters)
        {
            LogMessage(message, GetWindowsLogType(type), parameters: parameters);
        }

        void ILoger<ILogParameter>.Log(string message, Exception ex, params ILogParameter[] parameters)
        {
            LogError(message, ex, parameters: parameters);
        }

        void ILoger<ILogParameter>.Log(string message, Exception ex, MessageType type, params ILogParameter[] parameters)
        {
            LogError(message, ex, DefaultLogName, GetWindowsLogType(type), parameters);
        }

        #endregion

        void WriteLogMessage(string logName, string message, EventLogEntryType messageType)
        {
            //основная логика записи в журнал Windows
            try
            {
                if(message.Length > MaxMessageLength)
                {
                    //попытка записи в файл
                    var fileLoger = new FileLoger();
                    string errorMsg = "Достигнута max длина сообщения. Сообщение обрезано. Полное сообщение будет записано в файл " + fileLoger.PathName;

                    StringBuilder sb = new StringBuilder(errorMsg.Length + message.Length + 10);
                    sb.AppendLine(errorMsg).Append(message);

                    var cuttedMsg = sb.ToString(0, MaxMessageLength);
                    fileLoger.Log(message, GetECRLogType(messageType));
                    
                    //пишем обрезанное сообщение как обычно
                    message = cuttedMsg;
                }

                using (var log = new EventLog(logName))
                {
                    log.Source = _source;
                    log.WriteEntry(message, messageType);
                }
            }
            catch(Exception ex)
            {
                if (LogerError != null)
                    LogerError(this); //!!! achtung возможна рекурсия

                //try
                //{
                //    WriteLogMessage(DefaultLogName, LogMessageBuilder.AppendException(new StringBuilder("Возникло исключение при записи в журнал Windows"), ex).ToString(), EventLogEntryType.Error);
                //}
                //catch
                //{
                //    //никак не обрабатываем повторные ошибки записи в EventLog
                //}

            }
        }

    }
}
