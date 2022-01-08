using CarLeasingViewer.Models;
using System.Text;

namespace CarLeasingViewer.Log.Logers
{
    /// <summary>
    /// Логирование дней
    /// </summary>
    public class DayLoger : TypeLoger<WeekDay>
    {
        /// <summary>
        /// Только для служебных вызовов!!!
        /// </summary>
        public DayLoger() : base() { }
        /// <summary>
        /// Основной рабочий конструктор
        /// </summary>
        /// <param name="set">Набор логеров для записи</param>
        public DayLoger(LogSet set) : base(set) { }

        protected override StringBuilder AppendSubMessage(WeekDay instance, StringBuilder sb, string indent = null)
        {
            if (sb == null)
                sb = new StringBuilder();

            string baseIndent = indent;
            sb.Append(indent ?? string.Empty).AppendLine("День --------");
            sb.AppendLine();

            const string localIndent = "    ";
            indent = indent == null ? localIndent : indent + localIndent;

            sb.Append(indent).Append("Индекс: ").AppendLine();

            sb.AppendLine();
            sb.Append(baseIndent).AppendLine("---------------------");

            return sb;
        }
    }
}
