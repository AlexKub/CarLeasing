using System.Collections.Generic;
using System.Linq;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Общий набор Аренд авто для LeasingChart
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

        private IReadOnlyList<MonthHeaderModel> m_Monthes;
        /// <summary>
        /// Возвращает или задаёт месяцы
        /// </summary>
        public IReadOnlyList<MonthHeaderModel> Monthes { get { return m_Monthes; } set { m_Monthes = value; OnPropertyChanged(); } }

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

                            //leasing.Month = MonthHeader;
                        }

                        rowCount = distinctRowIndexes.Count;
                    }

                    RowsCount = rowCount;

                    OnPropertyChanged();
                }
            }
        }

        private IReadOnlyList<CarComment> m_Comments;
        /// <summary>
        /// Возвращает или задаёт Комментарии к авто
        /// </summary>
        public IReadOnlyList<CarComment> Comments { get { return m_Comments; } set { m_Comments = value; OnPropertyChanged(); } }

        void OnDataChanged()
        {
            CarModels = GetCarModels(m_data);
            RowsCount = CarModels.Count;
        }

        #region Перевод из одной модели данных в текущую

        /// <summary>
        /// Перевод из старой модели данныъ в новую (костыль)
        /// </summary>
        /// <param name="businesses">Набор занятости авто из БД или тестовый</param>
        public void ReMapBussinesses(IEnumerable<MonthBusiness> businesses)
        {
            CarModels = GetCarModels(businesses);
            Comments = GetComments(CarModels);

            //!!зависит от заполнения CarModels
            Leasings = GetLeasingModels(businesses);
        }

        IReadOnlyList<CarModel> GetCarModels(IEnumerable<MonthBusiness> data)
        {
            return data
                .SelectMany(mb => mb.CarBusiness)
            .Select(cb => cb.Name)
            .Distinct()
            .Select(name => new CarModel() { Text = name }).ToList();
        }

        IReadOnlyList<CarComment> GetComments(IReadOnlyList<CarModel> cars)
        {
            return cars.Select(car => new CarComment() { RowIndex = car.RowIndex, Comment = (car.Text + "_comment") }).ToList();
        }

        IReadOnlyList<LeasingElementModel> GetLeasingModels(IEnumerable<MonthBusiness> monthBuisnesses)
        {
            var leasingBarModels = new List<LeasingElementModel>();

            var index = 0;

            foreach (var business in monthBuisnesses)
            {
                foreach (var item in business.CarBusiness)
                {
                    var car = m_CarModels.FirstOrDefault(c => c.Text.Equals(item.Name));

                    leasingBarModels.AddRange(item.Business.Select(b => new LeasingElementModel() { Leasing = b, RowIndex = car == null ? 0 : car.RowIndex, DayColumnWidth = 21d }));
                }
                index++;
            }

            return leasingBarModels;
        }

        #endregion
    }
}
