using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace CarLeasingViewer
{
    /// <summary>
    /// Делегат методы получения строкового значения экземпляра
    /// </summary>
    /// <typeparam name="TObj">Тип</typeparam>
    /// <param name="instance">Экземпляр</param>
    /// <returns>Возвращает строковое представление экземпляра</returns>
    public delegate string StringValueDelegate<TObj>(TObj instance);

    /// <summary>
    /// Общая логика построения сообщений для логирования
    /// </summary>
    public static class LogMessageBuilder
    {
        const string WinLogerSource = nameof(LogMessageBuilder);

        /// <summary>
        /// Разделитель имени/значения в логе
        /// </summary>
        public static readonly string DefaultDelimiter = " | ";
        /// <summary>
        /// Заголовок для секции Параметры
        /// </summary>
        public static readonly string DefaultParametersHeader = "Параметры: -----------------";

        /// <summary>
        /// Логирование Параметра по умолчанию: "Имя: @Name@@Delimeter@Value:@Value@\r\n"
        /// </summary>
        /// <param name="p">Параметр</param>
        /// <param name="indent">Отступ</param>
        /// <returns>Возвращает новый экземпляр StringBuilder со схемой по умолчанию</returns>
        public static StringBuilder DefaultAppendParameter(StringBuilder sb, ILogParameter p, string indent = "")
        {
            if (sb == null)
                sb = new StringBuilder();
            if (p == null)
                return sb;

            var lpwd = p as ILogParamWithDelimiter;

            string delimiter = lpwd == null ? DefaultDelimiter : lpwd.Delimiter;

            try
            {
                sb.Append(indent).Append(p.Name).Append(delimiter).AppendLine(p.Value);
            }
            catch (Exception ex)
            {
                AppendLogParameterException(sb, p, ex, DefaultParametersHeader, indent, -1);
            }
            return sb;
        }

        /// <summary>
        /// Логика построения простых сообщений при логировании
        /// </summary>
        /// <param name="message">Вспомогательное сообщение</param>
        /// <param name="parameters">Параметры для логирования</param>
        /// <returns>Возвращает </returns>
        public static string BuildMessage(string message, string paramsHeader = null, string indent = "", params ILogParameter[] parameters)
        {
            //пустая ссылка вообще не информативна
            //поскольку она несёт в себе лишь информацию об ошибке в кодировании
            //просто ставим защиту от дурака
            if (message == null)
                message = string.Empty;

            try
            {
                if (paramsHeader == null)
                    paramsHeader = DefaultParametersHeader;

                if (parameters == null || parameters.Length == 0)
                    return message;

                StringBuilder sb = new StringBuilder();
                sb.Append(indent).AppendLine(message);

                return BuildParameters(sb, parameters, paramsHeader, indent).ToString();
            }
            catch (Exception ex)
            {
                return BuildMessage("Возникло исключение при построении сообщения", ex, indent, paramsHeader, new LogParameter("Предидущее сообщение", message));
            }
        }

        /// <summary>
        /// Логика построения сообщений с исключениями при логировании
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="parameters"></param>
        public static string BuildMessage(string message, Exception ex, string paramsHeader = null, string indent = "", params ILogParameter[] parameters)
        {
            return BuildMessage(new StringBuilder(), message, ex, paramsHeader, indent, parameters).ToString();
        }

        /// <summary>
        /// Логика построения сообщений с исключениями при логировании
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <param name="parameters"></param>
        public static StringBuilder BuildMessage(StringBuilder sb, string message, Exception ex, string paramsHeader = null, string indent = "", params ILogParameter[] parameters)
        {
            if (sb == null)
                sb = new StringBuilder();

            try
            {
                sb.Append(indent).AppendLine(message);

                BuildParameters(sb, parameters, paramsHeader, indent);

                if (ex == null)
                    return sb;

                AppendException(sb, ex, indent);
            }
            catch (Exception buildEx)
            {
                string subMessage = BuildMessage("Возникло исключение при построении сообщения логирования", paramsHeader, indent,
                    new LogParameter("Логируемое сообщение", message)
                    , new LogParameter("Логируемое исключение", GetObjectLogVal(ex, (e) => { return e.Message; })));

                WindowsLoger.LogError(WinLogerSource, subMessage, buildEx);
            }

            return sb;
        }

        /// <summary>
        /// Конкатинация параметров логирования
        /// </summary>
        /// <param name="sb">Экземпляр StringBuilder для конкатинации</param>
        /// <param name="parameters">Коллекция параметров</param>
        /// <param name="paramsHeader">Заголовок секции с параметрами</param>
        /// <param name="indent">Отступ для строк</param>
        /// <returns>Возвращает использованный для конкатинации экземпляр StringBuilder</returns>
        public static StringBuilder BuildParameters(StringBuilder sb, ILogParameter[] parameters, string paramsHeader = null, string indent = "")
        {
            if (sb == null)
                sb = new StringBuilder();

            try
            {
                if (paramsHeader == null)
                    paramsHeader = DefaultParametersHeader;

                if (parameters == null || parameters.Length == 0)
                    return sb;

                sb.Append(indent).AppendLine(paramsHeader);
                ILogParameter p = null;

                for (int i = 0; i < parameters.Length; i++)
                {
                    p = parameters[i];
                    if (p == null)
                        continue;

                    try
                    {
                        p.AppendParameter(sb, indent);
                    }
                    catch (Exception ex)
                    {
                        AppendLogParameterException(sb, p, ex, paramsHeader, indent, i);
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Возникло исключение при сборке параметров лог-сообщения";
                sb.Append(indent).AppendLine(msg);
                AppendException(sb, ex, indent);

                WindowsLoger.LogError(WinLogerSource, msg, ex);
            }

            return sb;
        }

        static void AppendLogParameterException(StringBuilder sb, ILogParameter p, Exception ex, string paramsHeader, string indent, int index)
        {
            /*
            логируем исключение при добавлении параметра в сообщение

            возможно получется невротбольшой лог - надеемся, что всё будет ок
            */

            string exCase = "<getException>";
            string pName = exCase;
            string pValue = exCase;

            //чуть отступаем для вложенных исключений
            var subExIndent = indent + "   ";
            var nameValueIndent = subExIndent + "    ";

            try
            {
                pName = p.Name;
            }
            catch (Exception nameEx)
            {
                sb.Append(BuildMessage("Возникло исключение при получении имени параметра логирования", nameEx, paramsHeader, nameValueIndent
                , new LogParameter("Тип параметра", p.GetType().FullName)));
            }

            try
            {
                pValue = p.Value;
            }
            catch (Exception valueEx)
            {
                //если с именем проблем не было, логируем ещё и имя
                var valueParams = pName == exCase
                    ? new LogParameter[] { new LogParameter("Тип параметра", p.GetType().FullName) }
                    : new LogParameter[] { new LogParameter("Тип параметра", p.GetType().FullName), new LogParameter("Имя параметра", p.Name) };

                sb.Append(BuildMessage("Возникло исключение при получении значения параметра логирования", valueEx, paramsHeader, nameValueIndent
                , valueParams));
            }

            //если с именем и значением проблем не было, логируем ещё их
            var subExParams = new List<LogParameter>();
            subExParams.Add(new LogParameter("Тип параметра", p.GetType().FullName));
            subExParams.Add(new LogParameter("Индекс параметра", index.ToString()));

            if (pName != exCase)
                subExParams.Add(new LogParameter("Имя параметра", pName));
            if (pValue != exCase)
                subExParams.Add(new LogParameter("Значение параметра", pValue));

            sb.Append(BuildMessage("Возникло исключение при построении сообщения логирования для параметра", ex, paramsHeader, subExIndent
                , subExParams.ToArray()));
        }

        public static StringBuilder AppendException(StringBuilder sb, Exception ex, string prevIndent = null)
        {
            if (sb == null)
                sb = new StringBuilder();

            if (ex == null)
                return sb;

            const string indent = "    ";
            string curIndent = string.IsNullOrEmpty(prevIndent) ? string.Empty : prevIndent + indent;

            sb.Append(curIndent).Append("Тип исключения | ").AppendLine(ex.GetType().FullName);
            sb.Append(curIndent).Append("Сообщение исключения: ").AppendLine(ex.Message);

            sb.Append(curIndent).Append("Стек вызова:");

            //стек может быть пуст
            if (string.IsNullOrEmpty(ex.StackTrace))
                sb.AppendLine(GetStringLogVal(ex.StackTrace));
            else
            {
                sb.AppendLine(); //при последнем Append'е не было конца строки
                using (TextReader stringReader = new StringReader(ex.StackTrace))
                {
                    string _LineText = stringReader.ReadLine();

                    while (_LineText != null)
                    {
                        sb.Append(curIndent).AppendLine(_LineText.Trim());
                        _LineText = stringReader.ReadLine();
                    }
                }
            }

            #region ReflectionTypeLoadException

            //исключение возникает при ошибке загрузке типа в сборке
            //причина специфического логирования - у этих исключений подробная информация в отдельном свойстве
            //подробнее о типе - https://msdn.microsoft.com/ru-ru/library/system.reflection.reflectiontypeloadexception(v=vs.110).aspx
            var refTypeLoadEx = ex as System.Reflection.ReflectionTypeLoadException;
            if(refTypeLoadEx != null)
            {
                sb.Append(curIndent).AppendLine("Исключения загрузчика: -----------------");
                var subIndent = curIndent + indent;
                sb.Append(subIndent).Append("Количество исключений: ");
                var loaderExceptions = refTypeLoadEx.LoaderExceptions;
                if (loaderExceptions == null || loaderExceptions.Length == 0)
                    sb.AppendLine("0");
                else
                {
                    var exCount = loaderExceptions.Length;
                    sb.AppendLine(exCount.ToString());

                    for (int i = 0; i < exCount; i++)
                    {
                        sb.Append(subIndent).Append("Исключение ").Append(i.ToString()).AppendLine(": -----------");
                        AppendException(sb, loaderExceptions[i], subIndent);
                    }
                }
            }

            #endregion

            if (ex.InnerException != null)
            {
                sb.Append(curIndent).AppendLine("Вложенное исключение: ---------------------");
                AppendException(sb, ex.InnerException, curIndent);
            }

            return sb;
        }

        /// <summary>
        /// Получение не пустого значения из строки для логирования
        /// </summary>
        /// <param name="val">Логируемое значение</param>
        /// <returns>Возвращает переданное значение, если оно не пустое. В противном случае, возвращает NULL или EMPTY</returns>
        public static string GetStringLogVal(string val)
        {
            return val == null ? "NULL" : string.IsNullOrEmpty(val) ? "EMPTY" : val;
        }

        /// <summary>
        /// Получение не пустого значения из объекта для логирования
        /// </summary>
        /// <typeparam name="TObj">Тип объекта</typeparam>
        /// <param name="val">Логируемое значение</param>
        /// <param name="del">Делегат на метод получения строкового представления объекта</param>
        /// <returns>Возвращает значение, полученное из делегата или NULL, если экземпляр пуст</returns>
        public static string GetObjectLogVal<TObj>(TObj val, StringValueDelegate<TObj> del)
        {
            return val == null ? "NULL" : del.Invoke(val);
        }

        public static string BuildIndent(string baseIndent, string appendIndent = "    ")
        {
            if (string.IsNullOrEmpty(baseIndent))
                return appendIndent;
            else
                return baseIndent + appendIndent;
        }
    }
}
