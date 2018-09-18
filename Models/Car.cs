namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Машина
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{ToString()}")]
    public class Car
    {
        /// <summary>
        /// Модель (Марка + модель)
        /// </summary>
        public string Model { get; private set; }

        /// <summary>
        /// ГосНомер
        /// </summary>
        public string Number { get; private set; }

        public Car(string model, string number)
        {
            Model = model;
            Number = number;
        }

        public override string ToString()
        {

            return string.IsNullOrEmpty(Number) ? Model : Model + $" ({Number})";
        }
    }
}
