using CarLeasingViewer.ViewModels;
using System;
using System.Collections.Generic;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Информация о ресурсе
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class ItemInfo : ViewModelBase
    {
        List<Leasing> m_businesses = new List<Leasing>();

        /// <summary>
        /// ID позиции (колонка No_ в Items)
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// Наименование авто
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Аренды
        /// </summary>
        public IList<Leasing> Leasings { get { return m_businesses; } }
        /// <summary>
        /// Месяц, в контексте которого рассаматриваем текущий жкземпляр
        /// </summary>
        public Month Month { get; set; }

        public Month[] Monthes { get; set; }

        /// <summary>
        /// Информация о нахождении в ремонте
        /// </summary>
        public MaintenanceInfo Maintenance { get; set; }
        /// <summary>
        /// Дата окончания ОСАГО
        /// </summary>
        public DateTime OSAGO_END { get; set; }
        /// <summary>
        /// Дата окончания КАСКО
        /// </summary>
        public DateTime KASKO_END { get; set; }

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
            return (string.IsNullOrEmpty(ID) ? "NO_ID" : ID) + " | " + (string.IsNullOrEmpty(Name) ? "NO_NAME" : Name);
        }

        /// <summary>
        /// Проверка на соответствие по уникальным ключам у экземпляров (ID|Имя)
        /// </summary>
        /// <param name="carBusiness">Экземпляр для сравнения</param>
        /// <returns>Возвращает соответствие уникального ключа экземпляров</returns>
        public bool EqualsID(ItemInfo carBusiness)
        {
            if (carBusiness == null)
                return false;

            if (string.IsNullOrEmpty(carBusiness.ID))
            {
                if (string.IsNullOrEmpty(Name))
                    return false;

                return Name.Equals(carBusiness.Name);
            }
            else
            {
                return carBusiness.ID.Equals(ID);
            }
        }

        public void Add(Leasing b)
        {
            m_businesses.Add(b);
        }
    }
}
