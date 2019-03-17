using System.Windows.Media;
using CarLeasingViewer.Interfaces;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Модель данных для Единицы занятости авто (полоски с текстом) на Canvas
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class CarModel : ViewModels.ViewModelBase, IIndexable, IHightlightable
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


                    if (Car == null)
                    {
                        Text = "NULL";
                    }
                    else
                    {
                        Text = Car.ToString();
                        Blocked = Car.Blocked;
                    }

                    OnPropertyChanged();
                }
            }
        }

        private bool m_Blocked;
        /// <summary>
        /// Возвращает или задаёт Флаг, что авто снято с учёта
        /// </summary>
        public bool Blocked { get { return m_Blocked; } set { m_Blocked = value; OnPropertyChanged(); } }

        private bool m_isMaintaining;
        /// <summary>
        /// Возвращает или задаёт Флаг занятости авто
        /// </summary>
        public bool IsMaintaining { get { return m_isMaintaining; } set { m_isMaintaining = value; OnPropertyChanged(); } }

        private string pv_Text;
        /// <summary>
        /// Возвращает или задаёт
        /// </summary>
        public string Text { get { return pv_Text; } set { if (pv_Text != value) { pv_Text = value; OnPropertyChanged(); } } }

        private int pv_MonthOffset;
        /// <summary>
        /// Возвращает или задаёт Смещение текущего месяца
        /// </summary>
        public int MonthOffset { get { return pv_MonthOffset; } set { pv_MonthOffset = value; OnPropertyChanged(); } }

        private CarPriceList pv_Price = CarPriceList.Default;
        /// <summary>
        /// Возвращает или задаёт Цену на машину
        /// </summary>
        public CarPriceList Price { get { return pv_Price; } set { pv_Price = value; OnPropertyChanged(); } }

        /// <summary>
        /// Индекс строки
        /// </summary>
        public int RowIndex { get; set; }

        #region IIndexable

        int IIndexable.Index { get => RowIndex; set => RowIndex = value; }

        #endregion

        #region IHightlightable

        private Brush pv_HightlightBrush;
        /// <summary>
        /// Возвращает или задаёт кисть подсветки
        /// </summary>
        public Brush HightlightBrush { get { return pv_HightlightBrush; } set { if (pv_HightlightBrush != value) { pv_HightlightBrush = value; OnPropertyChanged(); } } }

        /// <summary>
        /// Флаг подсветки
        /// </summary>
        public bool Hightlighted { get; set; }

        #endregion

        string DebugDisplay()
        {
            return RowIndex.ToString() + " | " + (string.IsNullOrEmpty(Text) ? "NO TEXT" : Text);
        }

        /// <summary>
        /// Обновление цены авто из БД
        /// </summary>
        public void UpdatePrice()
        {
            var test = App.TestMode && App.SearchSettings.TestData;
            if (!test)
                Price = DB_Manager.Default.GetCarPrice(pv_Car);
        }

        /// <summary>
        /// Новый экземпляр с теми же значениями свойств, кроме RowIndex
        /// </summary>
        /// <returns>Возвращает новый экземпляр с теми же значениями свойств, кроме RowIndex</returns>
        public CarModel Clone()
        {
            var newInstance = new CarModel();
            newInstance.pv_Text = pv_Text;
            newInstance.pv_Car = pv_Car;
            newInstance.pv_HightlightBrush = pv_HightlightBrush;
            newInstance.pv_MonthOffset = pv_MonthOffset;
            newInstance.pv_Price = pv_Price;

            return newInstance;
        }
    }
}
