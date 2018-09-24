using System;
using System.Collections.Generic;

namespace CarLeasingViewer
{
    /// <summary>
    /// Набор для логирования
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class LogSet : ILoger, IDisposable
    {
        readonly List<LogerPack> m_logers = new List<LogerPack>();
        readonly List<ILogParameter> m_parameters = new List<ILogParameter>();
        bool m_logError = false;

        public event Action<ILoger<ILogParameter>> LogerError;

        /// <summary>
        /// Настройки логирования
        /// </summary>
        public LogSettings Settings { get; set; }

        /// <summary>
        /// Дополнительные параметры при логировании
        /// </summary>
        public IEnumerable<ILogParameter> SetParameters { get { return m_parameters; } }

        /// <summary>
        /// Коллекция используемых конечных логеров
        /// </summary>
        public IEnumerable<LogerPack> LogerPacks { get { return m_logers; } }



        /// <summary>
        /// Создание набора с настройками по умолчанию
        /// </summary>
        public LogSet()
        {
            Settings = LogManager.Settings;
        }

        /// <summary>
        /// Добавление логера для логирования
        /// </summary>
        /// <param name="loger">Целевой логер для записи</param>
        public void AddLoger(ILoger loger)
        {
            m_logers.Add(new LogerPack(loger));
        }
        /// <summary>
        /// Добавление логера для логирования с указанными параметрами
        /// </summary>
        /// <param name="loger">Целевой логер для записи</param>
        public void AddLoger(LogerPack pack)
        {
            if (pack.Loger == null)
                throw new ArgumentNullException("Не допустимо добавление в набор логирования пустого экземпляра логера");
            m_logers.Add(pack);


            pack.Loger.LogerError += OnLogerError;
        }

        /// <summary>
        /// Добавляет параметр при логировании
        /// </summary>
        /// <param name="logParam">Добавляемый параметр</param>
        public void AddParameter(ILogParameter logParam)
        {
            m_parameters.Add(logParam);
        }

        #region Логирование

        /// <summary>
        /// Логирование сообщения со статусом MessageType.Info
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="parameters">Параметры сообщения</param>
        public void Log(string message, params ILogParameter[] parameters)
        {
            this.Log(message, MessageType.Info, parameters);
        }
        /// <summary>
        /// Логирование сообщения со статусом MessageType.Error
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="ex">Исключение</param>
        /// <param name="parameters">Параметры для логирования</param>
        public void Log(string message, Exception ex, params ILogParameter[] parameters)
        {
            this.Log(message, ex, MessageType.Error, parameters);
        }
        /// <summary>
        /// Логирование сообщения с определённым статусом
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="type">Статус сообщения</param>
        /// <param name="parameters">Параметры сообщения</param>
        public void Log(string message, MessageType type, params ILogParameter[] parameters)
        {
            InternalLog(message, null, type, null, false, false, parameters);
        }
        /// <summary>
        /// Логирование сообщения исключения с определённым статусом
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="ex">Исключение</param>
        /// <param name="type">Статус сообщения</param>
        /// <param name="parameters">Параметры для логирования</param>
        public void Log(string message, Exception ex, MessageType type, params ILogParameter[] parameters)
        {
            InternalLog(message, ex, type, null, false, false, parameters);
        }

        /// <summary>
        /// Принудительное логирование сообщения, без учёта уровня логирования, со статусом MessageType.Info
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="parameters">Параметры сообщения</param>
        public void ForceLog(string message, params ILogParameter[] parameters)
        {
            this.ForceLog(message, MessageType.Info, parameters);
        }
        /// <summary>
        /// Принудительное логирование сообщения, без учёта уровня логирования, со статусом MessageType.Error
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="ex">Исключение</param>
        /// <param name="parameters">Параметры для логирования</param>
        public void ForceLog(string message, Exception ex, params ILogParameter[] parameters)
        {
            this.ForceLog(message, ex, MessageType.Error, parameters);
        }
        /// <summary>
        /// Принудительное логирование сообщения, без учёта уровня логирования, с определённым статусом
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="type">Статус сообщения</param>
        /// <param name="parameters">Параметры сообщения</param>
        public void ForceLog(string message, MessageType type, params ILogParameter[] parameters)
        {
            InternalLog(message, null, type, null, false, true, parameters);
        }
        /// <summary>
        /// Принудительное логирование сообщения исключения, без учёта уровня логирования,  с определённым статусом
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="ex">Исключение</param>
        /// <param name="type">Статус сообщения</param>
        /// <param name="parameters">Параметры для логирования</param>
        public void ForceLog(string message, Exception ex, MessageType type, params ILogParameter[] parameters)
        {
            InternalLog(message, ex, type, null, false, true, parameters);
        }

        #region LogBy

        /// <summary>
        /// Логирование сообщения через указанный логер, со статусом MessageType.Info
        /// </summary>
        /// <param name="loger">Экземпляр логера для логирования</param>
        /// <param name="message">Сообщение</param>
        /// <param name="parameters">Параметры сообщения</param>
        public void LogBy(ILoger loger, string message, params ILogParameter[] parameters)
        {
            InternalLog(message, null, MessageType.Info, loger, true, false, parameters);
        }
        /// <summary>
        /// Логирование сообщения через указанный логер, со статусом MessageType.Info
        /// </summary>
        /// <param name="loger">Экземпляр логера для логирования</param>
        /// <param name="solo">Логировать только текущим логером или также всеми имеющимися в наборе</param>
        /// <param name="message">Сообщение</param>
        /// <param name="parameters">Параметры сообщения</param>
        public void LogBy(ILoger loger, bool solo, string message, params ILogParameter[] parameters)
        {
            InternalLog(message, null, MessageType.Info, loger, solo, false, parameters);
        }
        /// <summary>
        /// Логирование сообщения через указанный логер, со статусом MessageType.Error
        /// </summary>
        /// <param name="loger">Экземпляр логера для логирования</param>
        /// <param name="message">Сообщение</param>
        /// <param name="ex">Исключение</param>
        /// <param name="parameters">Параметры для логирования</param>
        public void LogBy(ILoger loger, string message, Exception ex, params ILogParameter[] parameters)
        {
            InternalLog(message, ex, MessageType.Info, loger, true, false, parameters);
        }
        /// <summary>
        /// Логирование сообщения через указанный логер, со статусом MessageType.Error
        /// </summary>
        /// <param name="loger">Экземпляр логера для логирования</param>
        /// <param name="solo">Логировать только текущим логером или также всеми имеющимися в наборе</param>
        /// <param name="message">Сообщение</param>
        /// <param name="ex">Исключение</param>
        /// <param name="parameters">Параметры для логирования</param>
        public void LogBy(ILoger loger, bool solo, string message, Exception ex, params ILogParameter[] parameters)
        {
            InternalLog(message, ex, MessageType.Info, loger, solo, false, parameters);
        }
        /// <summary>
        /// Логирование сообщения через указанный логер, с определённым статусом
        /// </summary>
        /// <param name="loger">Экземпляр логера для логирования</param>
        /// <param name="message">Сообщение</param>
        /// <param name="type">Статус сообщения</param>
        /// <param name="parameters">Параметры сообщения</param>
        public void LogBy(ILoger loger, string message, MessageType type, params ILogParameter[] parameters)
        {
            InternalLog(message, null, MessageType.Info, loger, true, false, parameters);
        }
        /// <summary>
        /// Логирование сообщения через указанный логер, с определённым статусом
        /// </summary>
        /// <param name="loger">Экземпляр логера для логирования</param>
        /// <param name="solo">Логировать только текущим логером или также всеми имеющимися в наборе</param>
        /// <param name="message">Сообщение</param>
        /// <param name="type">Статус сообщения</param>
        /// <param name="parameters">Параметры сообщения</param>
        public void LogBy(ILoger loger, bool solo, string message, MessageType type, params ILogParameter[] parameters)
        {
            InternalLog(message, null, MessageType.Info, loger, solo, false, parameters);
        }
        /// <summary>
        /// Логирование сообщения через указанный логер,  с определённым статусом
        /// </summary>
        /// <param name="loger">Экземпляр логера для логирования</param>
        /// <param name="message">Сообщение</param>
        /// <param name="ex">Исключение</param>
        /// <param name="type">Статус сообщения</param>
        /// <param name="parameters">Параметры для логирования</param>
        public void LogBy(ILoger loger, string message, Exception ex, MessageType type, params ILogParameter[] parameters)
        {
            InternalLog(message, ex, type, loger, true, false, parameters);
        }
        /// <summary>
        /// Логирование сообщения через указанный логер,  с определённым статусом
        /// </summary>
        /// <param name="loger">Экземпляр логера для логирования</param>
        /// <param name="solo">Логировать только текущим логером или также всеми имеющимися в наборе</param>
        /// <param name="message">Сообщение</param>
        /// <param name="ex">Исключение</param>
        /// <param name="type">Статус сообщения</param>
        /// <param name="parameters">Параметры для логирования</param>
        public void LogBy(ILoger loger, bool solo, string message, Exception ex, MessageType type, params ILogParameter[] parameters)
        {
            InternalLog(message, ex, type, loger, solo, false, parameters);
        }

        #endregion

        #region ForceLogBy

        /// <summary>
        /// Логирование сообщения через указанный логер, со статусом MessageType.Info
        /// </summary>
        /// <param name="loger">Экземпляр логера для логирования</param>
        /// <param name="message">Сообщение</param>
        /// <param name="parameters">Параметры сообщения</param>
        public void ForceLogBy(ILoger loger, string message, params ILogParameter[] parameters)
        {
            InternalLog(message, null, MessageType.Info, loger, true, true, parameters);
        }
        /// <summary>
        /// Логирование сообщения через указанный логер, со статусом MessageType.Info
        /// </summary>
        /// <param name="loger">Экземпляр логера для логирования</param>
        /// <param name="solo">Логировать только текущим логером или также всеми имеющимися в наборе</param>
        /// <param name="message">Сообщение</param>
        /// <param name="parameters">Параметры сообщения</param>
        public void ForceLogBy(ILoger loger, bool solo, string message, params ILogParameter[] parameters)
        {
            InternalLog(message, null, MessageType.Info, loger, solo, true, parameters);
        }
        /// <summary>
        /// Логирование сообщения через указанный логер, со статусом MessageType.Error
        /// </summary>
        /// <param name="loger">Экземпляр логера для логирования</param>
        /// <param name="message">Сообщение</param>
        /// <param name="ex">Исключение</param>
        /// <param name="parameters">Параметры для логирования</param>
        public void ForceLogBy(ILoger loger, string message, Exception ex, params ILogParameter[] parameters)
        {
            InternalLog(message, ex, MessageType.Info, loger, true, true, parameters);
        }
        /// <summary>
        /// Логирование сообщения через указанный логер, со статусом MessageType.Error
        /// </summary>
        /// <param name="loger">Экземпляр логера для логирования</param>
        /// <param name="solo">Логировать только текущим логером или также всеми имеющимися в наборе</param>
        /// <param name="message">Сообщение</param>
        /// <param name="ex">Исключение</param>
        /// <param name="parameters">Параметры для логирования</param>
        public void ForceLogBy(ILoger loger, bool solo, string message, Exception ex, params ILogParameter[] parameters)
        {
            InternalLog(message, ex, MessageType.Info, loger, solo, true, parameters);
        }
        /// <summary>
        /// Логирование сообщения через указанный логер, с определённым статусом
        /// </summary>
        /// <param name="loger">Экземпляр логера для логирования</param>
        /// <param name="message">Сообщение</param>
        /// <param name="type">Статус сообщения</param>
        /// <param name="parameters">Параметры сообщения</param>
        public void ForceLogBy(ILoger loger, string message, MessageType type, params ILogParameter[] parameters)
        {
            InternalLog(message, null, MessageType.Info, loger, true, true, parameters);
        }
        /// <summary>
        /// Логирование сообщения через указанный логер, с определённым статусом
        /// </summary>
        /// <param name="loger">Экземпляр логера для логирования</param>
        /// <param name="solo">Логировать только текущим логером или также всеми имеющимися в наборе</param>
        /// <param name="message">Сообщение</param>
        /// <param name="type">Статус сообщения</param>
        /// <param name="parameters">Параметры сообщения</param>
        public void ForceLogBy(ILoger loger, bool solo, string message, MessageType type, params ILogParameter[] parameters)
        {
            InternalLog(message, null, MessageType.Info, loger, solo, true, parameters);
        }
        /// <summary>
        /// Логирование сообщения через указанный логер,  с определённым статусом
        /// </summary>
        /// <param name="loger">Экземпляр логера для логирования</param>
        /// <param name="message">Сообщение</param>
        /// <param name="ex">Исключение</param>
        /// <param name="type">Статус сообщения</param>
        /// <param name="parameters">Параметры для логирования</param>
        public void ForceLogBy(ILoger loger, string message, Exception ex, MessageType type, params ILogParameter[] parameters)
        {
            InternalLog(message, ex, type, loger, true, true, parameters);
        }
        /// <summary>
        /// Логирование сообщения через указанный логер,  с определённым статусом
        /// </summary>
        /// <param name="loger">Экземпляр логера для логирования</param>
        /// <param name="solo">Логировать только текущим логером или также всеми имеющимися в наборе</param>
        /// <param name="message">Сообщение</param>
        /// <param name="ex">Исключение</param>
        /// <param name="type">Статус сообщения</param>
        /// <param name="parameters">Параметры для логирования</param>
        public void ForceLogBy(ILoger loger, bool solo, string message, Exception ex, MessageType type, params ILogParameter[] parameters)
        {
            InternalLog(message, ex, type, loger, solo, true, parameters);
        }

        #endregion

        /// <summary>
        /// Логика логирования сообщений
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="ex">Исключение</param>
        /// <param name="type">Статус сообщения</param>
        /// <param name="loger">Логер, через который логируем. Null - логируем через всех</param>
        /// <param name="single">Логировать только через указанный логер или через указанный и прочие, указанные в коллекции</param>
        /// <param name="force">Принудительно логирование (текущий уровень логирования не учитывается)</param>
        /// <param name="parameters">Параметры для логирования</param>
        private void InternalLog(string message, Exception ex, MessageType type, ILoger loger, bool single, bool force, params ILogParameter[] parameters)
        {
            /*
            основная логика логирования в проекте
            */

            //не пытаемся логировать при возникновении исключения в прошлый раз
            //если какой-то косяк - логирование прекращаем, чтобы не логировать миллион раз в случае чего
            if (m_logError)
                return; 

            try
            {
                //для НЕ принудительнологируемых
                //проверяем, есть ли разрешение на логирование
                if (!force)
                    if (!IsLogable(type))
                        return;

                //добавляем общие параметры для LogSet к текущим, переданным пользователем
                ILogParameter[] setParams = ConcatParameters(parameters);

                bool logByAll = loger == null; //если конкретный логер не указан, полюбому логируем всеми

                if (loger != null) //если указан логер, логируем через него
                {
                    if (ex == null)
                        loger.Log(message, type, setParams);
                    else
                        loger.Log(message, ex, type, setParams);

                    logByAll = !single; //если не указан single, логируем также и прочими
                }

                if (logByAll) //логируем всеми логерами в наборе
                {
                    foreach (var pack in m_logers)
                    {
                        //проверяем, есть ли разрешение для текущего логера
                        if (pack.MessageTypes != null)
                            if (!force)
                                if (!IsLogable(pack.MessageTypes))
                                    continue;

                        //добавляем параметры текущего логера
                        if (pack.AdditionalProperties != null)
                            setParams = ConcatParameters(pack.AdditionalProperties, setParams);

                        //логируем
                        if (ex == null)
                            pack.Loger.Log(message, type, setParams);
                        else
                            pack.Loger.Log(message, ex, type, setParams);
                    }
                }
            }
            catch (Exception logEx)
            {
                m_logError = true;

                if (ex != null)
                    WindowsLoger.LogError("LogSet", message, ex, parameters);
                else
                    WindowsLoger.LogMessage("LogSet", message, WindowsLoger.GetWindowsLogType(type), parameters);

                WindowsLoger.LogError("LogSet", "Возникло исключение при попытке записи в журнал логирования", logEx);

                if (LogerError != null)
                    LogerError(this);
            }
        }

        #endregion

        ILogParameter[] ConcatParameters(params ILogParameter[] parameters)
        {
            return ConcatParameters(m_parameters.ToArray(), parameters);
        }
        ILogParameter[] ConcatParameters(ILogParameter[] first, ILogParameter[] second)
        {
            //объединение параметров, переданных ранее
            //с параметрами, установленными в наборе
            ILogParameter[] setParams = null;
            if (first == null || first.Length == 0)
                return second;
            if (second == null || second.Length == 0)
                return first;


            //добавляем параметры к уже имеющимся
            var newParams = new List<ILogParameter>();
            newParams.AddRange(first);
            newParams.AddRange(second);
            setParams = newParams.ToArray();


            return setParams;
        }

        /// <summary>
        /// Проверка наличия разрешения на логирование сообщений
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsLogable(MessageType type)
        {
            /*
            предполагается, что уровень текущего логирования (ак правило, для текущего домена)
            берётся из настроек и устанавливается в текущий Set
            таким образом, логируются только сообщения типа, указанного в настройках/при инициализации Set'a
            */

            var logLvl = (LogLevel)LogManager.Settings.Log_Level;
            switch (type)
            {
                case MessageType.Debug:
                    return logLvl == LogLevel.Debug;
                case MessageType.Info:
                    return logLvl == LogLevel.Debug || logLvl == LogLevel.Hight;
                case MessageType.Warning:
                    return logLvl == LogLevel.Debug || logLvl == LogLevel.Hight || logLvl == LogLevel.Medium;
                case MessageType.Error:
                default:
                    return true;
            }

        }
        /// <summary>
        /// Проверка наличия разрешения на логирование сообщений
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsLogable(MessageType[] types)
        {
            foreach (var type in types)
                if (!IsLogable(type))
                    return false;

            return true;
        }

        /// <summary>
        /// Получает новый экземпляр Набора с теми же настройками/логерами/параметрами
        /// </summary>
        /// <returns>Возвращает новый экземпляр с теми же настройками</returns>
        public LogSet GetCopy()
        {
            var copy = new LogSet();

            if (this.m_logers.Count != copy.m_logers.Count)
            {
                copy.m_logers.Clear();
                copy.m_logers.AddRange(this.m_logers);
            }

            if (this.m_parameters.Count != copy.m_parameters.Count)
            {
                copy.m_parameters.Clear();
                copy.m_parameters.AddRange(this.m_parameters);
            }

            return copy;
        }

        string DebugDisplay()
        {
            return "Level: " + Settings == null ? "NULL Settings" : Settings.Log_Level.ToString() + " | Count: " + m_logers.Count.ToString();
        }

        void OnLogerError(ILoger<ILogParameter> loger)
        {
            //при возникновении ошибки во время логирования
            LogerPack pack = null;

            foreach (var p in m_logers)
                if (p.Loger != null)
                {
                    if (p.Loger == loger)
                    {
                        pack = p;
                        break;
                    }
                }

            //удаляем косячный логер
            pack.Loger.LogerError -= OnLogerError;
            m_logers.Remove(pack);

            WindowsLoger.LogMessage("LogSet", "Логер был исключён из набора из-за исключения при логировании", System.Diagnostics.EventLogEntryType.Warning
                , new LogParameter("Тип логера", pack.Loger.GetType().FullName));

            if (m_logers.Count == 0)
            {
                if (!pack.Loger.GetType().Equals(typeof(WindowsLoger)))
                {
                    AddLoger(new WindowsLoger("LogSetGenerated"));

                    WindowsLoger.LogMessage("LogSet", "Логирование было переключено на журнал Windows из-за отсутствия других логеров в Наборе", System.Diagnostics.EventLogEntryType.Information
                       , new LogParameter("Тип логера", pack.Loger.GetType().FullName));
                }
            }
        }

        public void Dispose()
        {
            if (m_logers != null)
            {
                foreach (var pack in m_logers)
                {
                    if (pack != null && pack.Loger != null)
                    {
                        pack.Loger.LogerError -= OnLogerError;
                    }
                }
            }
        }

        /// <summary>
        /// Логер с настройками
        /// </summary>
        public class LogerPack
        {
            /// <summary>
            /// Логер
            /// </summary>
            public ILoger Loger { get; private set; }
            /// <summary>
            /// Дополнительные параметры, добавляемые к сообщениям этого логера
            /// </summary>
            public ILogParameter[] AdditionalProperties { get; private set; }
            /// <summary>
            /// Типы сообщений, логируемые текущим логером. Null - все типы
            /// </summary>
            public MessageType[] MessageTypes { get; private set; }

            public LogerPack(ILoger loger, MessageType[] mTypes = null, params ILogParameter[] parameters)
            {
                Loger = loger;
                MessageTypes = mTypes == null ? null : mTypes.Length == 0 ? null : mTypes;
                AdditionalProperties = parameters == null ? new ILogParameter[0] : parameters;
            }
        }

    }
}
