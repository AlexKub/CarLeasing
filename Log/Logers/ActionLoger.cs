using System;

namespace CarLeasingViewer
{

    /// <summary>
    /// Базовый Логер с переопределённым методом логирования
    /// </summary>
    public class ActionLoger : ILoger
    {
        readonly Action<string> m_logAction;

        /// <summary>
        /// СОздание нового логера с указанным делегатом логирования
        /// </summary>
        /// <param name="logAction">Делегат логирования (передаём метод логирования)</param>
        public ActionLoger(Action<string> logAction)
        {
            if (logAction == null)
                throw new ArgumentNullException("Создание логера по пустому делегату действия не предусмотрено");

            m_logAction = logAction;
        }

        public event Action<ILoger<ILogParameter>> LogerError;

        public void Log(string message, params ILogParameter[] parameters)
        {
            var logMessage = LogMessageBuilder.BuildMessage(message, parameters: parameters);

            if (m_logAction != null)
                m_logAction(logMessage);
        }

        public void Log(string message, MessageType type, params ILogParameter[] parameters)
        {
            Log(message, parameters); //TO DO
        }

        public void Log(string message, Exception ex, params ILogParameter[] parameters)
        {
            var logMessage = LogMessageBuilder.BuildMessage(message, ex, parameters: parameters);

            if (m_logAction != null)
                m_logAction(logMessage);
        }

        public void Log(string message, Exception ex, MessageType type, params ILogParameter[] parameters)
        {
            Log(message, ex, parameters); //TO DO
        }
    }
}
