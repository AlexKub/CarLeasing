using System;
using System.Text;

namespace CarLeasingViewer
{
    /// <summary>
    /// Параметр для логирования
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class LogParameter : ILogParameter
    {
        /// <summary>
        /// Имя параметра
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Значение параметра
        /// </summary>
        public string Value { get; private set; }

        public LogParameter(string name, string value)
        {
            Name = name;
            Value = LogMessageBuilder.GetStringLogVal(value);
        }

        string ILogParameter.Name
        {
            get
            {
                return Name;
            }
        }

        string ILogParameter.Value
        {
            get
            {
                return Value;
            }
        }

        StringBuilder ILogParameter.AppendParameter(StringBuilder sb, string indent)
        {
            return LogMessageBuilder.DefaultAppendParameter(sb, this, indent);
        }

        public override string ToString()
        {
            return LogMessageBuilder.DefaultAppendParameter(new StringBuilder(), this).ToString();
        }

        string DebugDisplay()
        {
            return ((string.IsNullOrEmpty(Name) ? "NO_NAME" : Name) 
                + LogMessageBuilder.DefaultDelimiter 
                + ((Value == null) ? "NULL_VAL" : Value));
        }
    }
}
