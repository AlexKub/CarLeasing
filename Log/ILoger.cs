using System;

namespace CarLeasingViewer
{
    /// <summary>
    /// Обобщённый интерфейс
    /// </summary>
    public interface ILoger : ILoger<ILogParameter>
    {

    }

    /// <summary>
    /// Общая логика логирования
    /// </summary>
    /// <typeparam name="TParam">Тип логируемого параметра</typeparam>
    public interface ILoger<TParam> where TParam : ILogParameter
    {
        /// <summary>
        /// При возникновении ошибки во время логирования
        /// </summary>
        event Action<ILoger<TParam>> LogerError;

        /// <summary>
        /// Логирование обычного сообщения (MessageType.Information)
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="parameters">Параметры сообщения</param>
        void Log(string message, params TParam[] parameters);
        /// <summary>
        /// Логирование сообщения определённого типа
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="type">Тип сообщения</param>
        /// <param name="parameters">Параметры сообщения</param>
        void Log(string message, MessageType type, params TParam[] parameters);
        /// <summary>
        /// Логирование исключения с типом MessageType.Error
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="ex">Экземпляр исключения</param>
        /// <param name="parameters">Параметры логирования</param>
        void Log(string message, Exception ex, params TParam[] parameters);
        /// <summary>
        /// Логирование исключения определённого типа
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="ex">Экземпляр исключения</param>
        /// <param name="type">Тип сообщения</param>
        /// <param name="parameters">Параметры логирования</param>
        void Log(string message, Exception ex, MessageType type, params TParam[] parameters);
    }
}
