using System;
using System.IO;
using System.Text;

namespace CarLeasingViewer
{
    /// <summary>
    /// Логирование в файл
    /// </summary>
    public class FileLoger : ILoger
    {
        const string LogSource = nameof(FileLoger);
        static WindowsLoger m_winLoger = new WindowsLoger(LogSource);

        /// <summary>
        /// Сегодняшняя дата в формате yyyy-MM-dd
        /// </summary>
        public static readonly string DefaultFileName = DateTime.Today.ToString("yyyy-MM-dd") + ".txt";
        /// <summary>
        /// Пустая строка. По умолчанию - текущий каталог приложения\Log
        /// </summary>
        public static readonly string DefaultLogFolder = Path.Combine(GetLogSettingsFolder(), "Log");

        public event Action<ILoger<ILogParameter>> LogerError;

        /// <summary>
        /// Имя файла лога
        /// </summary>
        public string FileName { get; protected set; }
        /// <summary>
        /// Папка логирования
        /// </summary>
        public string LogFolder { get; protected set; }

        /// <summary>
        /// Логер в файл по указанному пути
        /// </summary>
        /// <param name="fileName">Имя файла лога</param>
        /// <param name="logPath">Каталог</param>
        public FileLoger(string fileName = null, string logPath = null)
        {

            FileName = string.IsNullOrEmpty(fileName) ? DefaultFileName : fileName;
            LogFolder = logPath == null ? DefaultLogFolder : logPath;

            InitFolder(LogFolder);
        }

        /// <summary>
        /// Безопасное создание нового каталога
        /// </summary>
        /// <param name="folder">Полное имя нового каталога</param>
        protected void InitFolder(string folder)
        {
                try
                {
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                }
                catch (Exception ex)
                {
                    m_winLoger.LogError("Возникло исключение при попытке создания каталога логирования", ex
                        , new LogParameter("Каталог", LogMessageBuilder.GetStringLogVal(folder)));
                }
        }

        /// <summary>
        /// Возвращает полное имя файла
        /// </summary>
        public string PathName
        {
            get { return Path.Combine(LogFolder, FileName); }
        }

        /// <summary>
        /// Логирование сообщения в текущий файл
        /// </summary>
        /// <param name="message">Сообщение для логирования</param>
        /// <param name="parameters">Параметры логирования</param>
        public void Log(string message, params ILogParameter[] parameters)
        {
            Log(PathName, message, parameters);
        }
        /// <summary>
        /// Логирование сообщения в указанный файл
        /// </summary>
        /// <param name="pathName">Путь к файлу</param>
        /// <param name="message">Сообщение для логирования</param>
        /// <param name="parameters">Параметры логирования</param>
        public void Log(string pathName, string message, params ILogParameter[] parameters)
        {
            try
            {
                var logMessage = LogMessageBuilder.BuildMessage(message, parameters: parameters);
                logMessage = BuildFileMessage(logMessage);

                File.AppendAllText(pathName, logMessage);
            }
            catch (Exception ex)
            {
                WindowsLoger.LogError(LogSource, "Возникло исключение при логировании в файл", ex);
                if (LogerError != null)
                    LogerError(this);
            }
        }

        /// <summary>
        /// Логирование сообщения в текущий файл
        /// </summary>
        /// <param name="message">Сообщение для логирования</param>
        /// <param name="ex">Логируемое исключение</param>
        /// <param name="parameters">Параметры логирования</param>
        public void Log(string message, Exception ex, params ILogParameter[] parameters)
        {
            Log(PathName, message, ex, parameters);
        }
        /// <summary>
        /// Логирование сообщения в указанный файл
        /// </summary>
        /// <param name="pathName">Путь к файлу</param>
        /// <param name="message">Сообщение для логирования</param>
        /// <param name="ex">Логируемое исключение</param>
        /// <param name="parameters">Параметры логирования</param>
        public void Log(string pathName, string message, Exception ex, params ILogParameter[] parameters)
        {
            try
            {
                var logMessage = LogMessageBuilder.BuildMessage(message, ex, parameters: parameters);
                logMessage = BuildFileMessage(logMessage);

                File.AppendAllText(pathName, logMessage);
            }
            catch (Exception subEx)
            {
                WindowsLoger.LogError(LogSource, "Возникло исключение при логировании в файл", subEx);
                if (LogerError != null)
                    LogerError(this);
            }
        }

        static string GetLogSettingsFolder()
        {
            string folder = string.Empty;
            try
            {
                folder = LogManager.Settings.FileLogingFolder;

                if (string.IsNullOrEmpty(folder))
                    return string.Empty;

                //if(!Core_2_0.Utilites.ValidatePathName(ref folder))
                //{
                //    m_winLoger.LogMessage("Указан некорреткный каталог в файле настроек логирования. Взят путь по умолчанию", MessageType.Warning,
                //        new LogParameter("Каталог", folder));
                //    folder = string.Empty;
                //}
            }
            catch(Exception ex)
            {
                m_winLoger.LogError("Возникло исключение при получении корневого каталога для логирования", ex,
                        new LogParameter("Каталог", folder));
                folder = string.Empty;
            }

            return folder;
        }

        static string BuildFileMessage(string rawMessage)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(); //отступ перед сообщением

            sb.Append(DateTime.Now.ToString()).AppendLine("+++++++++++++++++++++++++++++++");
            sb.AppendLine(rawMessage);
            sb.AppendLine("----------------------------------------------------------");

            return sb.ToString();
        }

        public void Log(string message, MessageType type, params ILogParameter[] parameters)
        {
            Log(message, parameters);
        }

        public void Log(string message, Exception ex, MessageType type, params ILogParameter[] parameters)
        {
            Log(message, ex, parameters);
        }
    }
}
