using CarLeasingViewer.ViewModels;
using System.Collections.Generic;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Аренда авто
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class CarBusiness : ViewModelBase
    {
        List<Business> m_businesses = new List<Business>();

        /// <summary>
        /// ID позиции (колонка No_ в Items)
        /// </summary>
        public string ItemNo { get; set; }
        /// <summary>
        /// Наименование авто
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Позиции Аренды авто
        /// </summary>
        public IList<Business> Business { get { return m_businesses; } }
        /// <summary>
        /// Месяц, в контексте которого рассаматриваем текущий жкземпляр
        /// </summary>
        public Month Month { get; set; }

        public Month[] Monthes { get; set; }

        /// <summary>
        /// Количество аренд на текущий месяц
        /// </summary>
        public int Count { get { return m_businesses.Count; } }

        private double pv_DaysHeaderWidth;
        /// <summary>
        /// Возвращает или задаёт Ширину строки в заголовке
        /// </summary>
        public double DaysHeaderWidth { get { return pv_DaysHeaderWidth; } set { if (pv_DaysHeaderWidth != value) { pv_DaysHeaderWidth = value; OnPropertyChanged(); } } }

        string DebugDisplay()
        {
            return (string.IsNullOrEmpty(ItemNo) ? "NO_ID" : ItemNo) + " | " + (string.IsNullOrEmpty(Name) ? "NO_NAME" : Name);
        }

        /// <summary>
        /// Проверка на соответствие по уникальным ключам у экземпляров (ID|Имя)
        /// </summary>
        /// <param name="carBusiness">Экземпляр для сравнения</param>
        /// <returns>Возвращает соответствие уникального ключа экземпляров</returns>
        public bool EqualsID(CarBusiness carBusiness)
        {
            if (carBusiness == null)
                return false;

            if(string.IsNullOrEmpty(carBusiness.ItemNo))
            {
                if (string.IsNullOrEmpty(Name))
                    return false;

                return Name.Equals(carBusiness.Name);
            }
            else
            {
                return carBusiness.ItemNo.Equals(ItemNo);
            }
        }

        public void Add(Business b)
        {
            m_businesses.Add(b);
        }
    }
}
