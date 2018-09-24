using CarLeasingViewer.Log;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Threading;

namespace CarLeasingViewer
{
    /// <summary>
    /// Логирование для экземпляра конкретного типа
    /// </summary>
    /// <typeparam name="TType">Логируемый тип</typeparam>
    public abstract class TypeLoger<TType> : IDisposable
    {
        protected readonly LogSet m_baseLoger;

        /// <summary>
        /// Основной набор, через который осуществляется логирование
        /// </summary>
        public LogSet TargetSet { get { return m_baseLoger; } }

        /// <summary>
        /// Инициализация без логера (только для вызова переопределённых методов)
        /// </summary>
        protected TypeLoger()
        {
            //без логера
        }
        /// <summary>
        /// Основной рабочий конструктор
        /// </summary>
        /// <param name="baseLoger">Набор логеров для логирования</param>
        public TypeLoger(LogSet baseLoger)
        {
            if (baseLoger == null)
                m_baseLoger = GetDefaultLoger<TypeLoger<TType>>();

            m_baseLoger = baseLoger;
        }

        public void Log(TType instance, string message, params ILogParameter[] parameters)
        {
            Log(instance, message, MessageType.Info, parameters);
        }

        public void Log(TType instance, string message, MessageType messageType, params ILogParameter[] parameters)
        {
            var typeMessage = ConcatLogMessage(message, instance);

            m_baseLoger.Log(typeMessage, messageType, parameters);

            AfterLog(instance);
        }

        public void Log(TType instance, string message, Exception ex, params ILogParameter[] parameters)
        {
            Log(instance, message, ex, MessageType.Error, parameters);
        }

        public void Log(TType instance, string message, Exception ex, MessageType messageType, params ILogParameter[] parameters)
        {
            var typeMessage = ConcatLogMessage(message, instance);

            m_baseLoger.Log(typeMessage, ex, messageType, parameters);

            AfterLog(instance);
        }

        /// <summary>
        /// Объединение начального текста сообщения с текстом от логируемого экземпляра
        /// </summary>
        /// <param name="baseMessage">Начальный текст сообщения</param>
        /// <param name="instance">Логируемый экземпляр</param>
        /// <param name="indent">Отступ для вложенного сообщения</param>
        /// <returns>Возвращает общее сообщение для логирования</returns>
        public string ConcatLogMessage(string baseMessage, TType instance, string indent = null)
        {
            //обединение текста исходного сообщения с данными логируемого экземпляра
            string opMessage = baseMessage;
            try
            {
                //основная логика
                var sb = new StringBuilder();
                sb.Append("Сообщение: ").AppendLine(LogMessageBuilder.GetStringLogVal(baseMessage)); //впереди добавляем сообщение
                sb.AppendLine();

                AppendSubMessage(instance, sb, indent);

                opMessage = sb.ToString();
            }
            catch (Exception ex)
            {
                m_baseLoger.Log("Возникло исключение при построении дополнительного сообщения для типа. Дополнительные данные не будут залогированы", ex
                    , new LogParameter("Логируемый тип", typeof(TType).FullName)
                    , new LogParameter("Основное сообщение", baseMessage));
            }

            return opMessage;
        }

        /// <summary>
        /// Генерация сообщения для экземпляра
        /// </summary>
        /// <param name="instance">Логируемый экземпляр</param>
        /// <param name="baseMessage">Исходный текст сообщение</param>
        /// <returns>Возвращает итоговое сообщение для логирования</returns>
        protected abstract StringBuilder AppendSubMessage(TType instance, StringBuilder sb, string indent = null);

        public static string BuildMessage<TLoger>(TType instance, string indent = "")
            where TLoger : TypeLoger<TType>, new()
        {
            var loger = new TLoger();

            StringBuilder sb = new StringBuilder();

            return loger.AppendSubMessage(instance, sb, indent).ToString();
        }

        public static StringBuilder AppendMessage<TLoger>(TType instance, StringBuilder sb, string indent = "")
            where TLoger : TypeLoger<TType>, new()
        {
            var loger = new TLoger();

            if (sb == null)
                sb = new StringBuilder();

            return loger.AppendSubMessage(instance, sb, indent);
        }

        /// <summary>
        /// Действия после логирования
        /// </summary>
        protected virtual void AfterLog(TType value) { }

        static LogSet GetDefaultLoger<TLoger>() where TLoger : TypeLoger<TType>
        {
            return LogManager.GetDefaultLogSet<TLoger>();
        }

        public static string BuildClassLogMessage(TType instance, string indent = null)
        {
            return AppendClassLogMessage(new StringBuilder(500), instance, indent).ToString();
        }

        public static StringBuilder AppendClassLogMessage(StringBuilder sb, TType instance, string indent = "")
        {
            if (sb == null)
                sb = new StringBuilder(500);

            return sb;
            //var curentIndent = LogMessageBuilder.BuildIndent(indent);
            //
            //var classType = instance.GetType();
            //var attr = instance.GetType().GetCustomAttribute<LogedClassAttribute>();
            //
            //var standartSearch = BindingFlags.Public | BindingFlags.Instance;
            //var standartTypes = MemberTypes.Property; ;
            //if (attr != null)
            //{
            //    standartSearch = attr.LogMembersSearch;
            //    standartTypes = attr.LogMemberTypes;
            //}
            //
            //var logMembers = classType.GetMembers(standartSearch).Where(m => (m.MemberType & standartTypes) != 0);
            //
            //sb.AppendLine();
            //sb.Append(indent).Append("Состояние экземпляра ").Append(classType.FullName).AppendLine("-----------");
            //if (logMembers.Count() > 0)
            //{
            //    var dispatcher = (instance as DispatcherObject)?.Dispatcher;
            //
            //    if (dispatcher != null)
            //    {
            //        dispatcher.Invoke(() =>
            //        {
            //            foreach (var member in logMembers)
            //            {
            //                if (member.GetCustomAttribute<LogIgnoreAttribute>() != null)
            //                    continue;
            //
            //                var prop = member as PropertyInfo;
            //            }
            //        });
            //    }
            //}
            //else
            //{
            //    sb.AppendLine();
            //    sb.Append(curentIndent).AppendLine("MEMBERS NOT FOUND");
            //    sb.AppendLine();
            //}
            //sb.Append(indent).Append("--------------------------------------------------------------------------");

        }

        static StringBuilder AppendMember<TAttr>(StringBuilder sb, TAttr memberAttr, string indent, object value)
            where TAttr : Log.LogedMemberAttribute
        {
            return sb.Append(indent).Append(memberAttr.LogName).Append(": ").AppendLine(memberAttr.ConvertValue(value));
        }

        public void Dispose()
        {
            if (m_baseLoger != null)
            {
                m_baseLoger.Dispose();
            }
        }
    }
}
