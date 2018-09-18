using System;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Модель для заголовка с машиной в списке
    /// </summary>
    public class CarHeaderModel : ViewModels.ViewModelBase, IIndexable
    {
        private Car pv_Car;
        /// <summary>
        /// Возвращает или задаёт
        /// </summary>
        public Car Car
        {
            get { return pv_Car; }
            set
            {
                if (pv_Car != value)
                {
                    pv_Car = value;

                    Text = Car == null ? "NULL" : Car.ToString();

                    OnPropertyChanged();
                }
            }
        }

        private string pv_Text;
        /// <summary>
        /// Возвращает или задаёт
        /// </summary>
        public string Text { get { return pv_Text; } set { if (pv_Text != value) { pv_Text = value; OnPropertyChanged(); } } }

        /// <summary>
        /// Индекс строки
        /// </summary>
        public int RowIndex { get; set; }

        public void SetIndex(int index)
        {
            RowIndex = index;
        }
    }
}
