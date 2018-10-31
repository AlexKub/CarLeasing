namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Машина
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{ToString()}")]
    public class Car
    {
        /// <summary>
        /// ID машины
        /// </summary>
        public string No { get; set; }

        /// <summary>
        /// Модель (Марка + модель)
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// Модель
        /// </summary>
        public string Model { get; private set; }

        /// <summary>
        /// ГосНомер
        /// </summary>
        public string Number { get; private set; }

        /// <summary>
        /// Авто заблокировано
        /// </summary>
        public bool Blocked { get; set; }

        public Car(string model, string number)
        {
            Model = model;
            Number = number;

            FullName = ToString();
        }

        public override string ToString()
        {

            return string.IsNullOrEmpty(Number) ? Model : Model + $" ({Number})";
        }
    }
}
