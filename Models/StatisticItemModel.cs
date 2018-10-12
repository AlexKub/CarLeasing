namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Модель одного юнита данных в статистике
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class StatisticItemModel
    {
        /// <summary>
        /// Имя юнита
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Значение юнита
        /// </summary>
        public string Value { get; set; }

        public StatisticItemModel(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Text
        {
            get
            {
                if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Value))
                    return string.Empty;

                return Name + ": " + Value + ";";
            }
        }

        string DebugDisplay()
        {
            return string.IsNullOrEmpty(Text) ? "EMPTY" : Text;
        }
    }
}
