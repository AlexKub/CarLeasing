using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Общий набор Аренд авто для View
    /// </summary>
    public class LeasingSet : ViewModels.ViewModelBase
    {
        private int m_RowsCount;
        /// <summary>
        /// Возвращает или задаёт Количество отрисовываемых строк 
        /// </summary>
        public int RowsCount { get { return m_RowsCount; } set { m_RowsCount = value; OnPropertyChanged(); } }

        IEnumerable<MonthBusiness> m_data;
        /// <summary>
        /// Данные Аренды
        /// </summary>
        public IEnumerable<MonthBusiness> Data
        {
            get { return m_data; }
            set
            {
                if (m_data != null && m_data != value)
                {
                    m_data = value;
                    OnDataChanged();
                }
            }
        }

        private IReadOnlyList<CarModel> m_CarModels;
        /// <summary>
        /// Возвращает или задаёт набор моделей авто для View
        /// </summary>
        public IReadOnlyList<CarModel> CarModels { get { return m_CarModels; } set { m_CarModels = value; OnPropertyChanged(); } }

        private IReadOnlyList<LeasingElementModel> pv_Leasings;
        /// <summary>
        /// Возвращает или задаёт набор занятости автомобилей в текущем месяце
        /// </summary>
        public IReadOnlyList<LeasingElementModel> Leasings
        {
            get { return pv_Leasings; }
            set
            {
                if (pv_Leasings != value)
                {
                    pv_Leasings = value;
                    //GridIndexHelper.SetIndexes(value);

                    //проставляем количество строк, на которые будут биндится модели
                    var rowCount = 0;

                    if (value != null)
                    {
                        var distinctRowIndexes = new List<int>(100);
                        foreach (var leasing in value)
                        {
                            if (!distinctRowIndexes.Contains(leasing.RowIndex))
                                distinctRowIndexes.Add(leasing.RowIndex);

                            leasing.Month = MonthHeader;
                        }

                        rowCount = distinctRowIndexes.Count;
                    }

                    RowsCount = rowCount;

                    OnPropertyChanged();
                }
            }
        }

        IReadOnlyList<CarModel> GetCarModels(IEnumerable<MonthBusiness> data)
        {
            return data
                .SelectMany(mb => mb.CarBusiness)
            .Select(cb => cb.Name)
            .Distinct()
            .Select(name => new CarModel() { Text = name }).ToList();
        }

        void OnDataChanged()
        {
            CarModels = GetCarModels(m_data);
            RowsCount = CarModels.Count;
        }
    }
}
