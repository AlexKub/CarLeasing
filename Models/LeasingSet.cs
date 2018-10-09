using System;
using System.Collections.Generic;
using System.Linq;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Событие набора Занятости авто
    /// </summary>
    /// <param name="set">Набор, в котоом произошло изменение</param>
    public delegate void LeasingSetEvent(LeasingSet set);

    /// <summary>
    /// Общий набор Аренд авто для LeasingChart
    /// </summary>
    public class LeasingSet : ViewModels.ViewModelBase, IDisposable
    {
        private int m_RowsCount;
        /// <summary>
        /// Возвращает или задаёт Количество отрисовываемых строк 
        /// </summary>
        public int RowsCount { get { return m_RowsCount; } set { m_RowsCount = value; OnPropertyChanged(); } }

        private int m_DaysCount;
        /// <summary>
        /// Возвращает или задаёт Количество дней
        /// </summary>
        public int DaysCount { get { return m_DaysCount; } set { m_DaysCount = value; OnPropertyChanged(); } }

        IEnumerable<MonthBusiness> m_data;
        /// <summary>
        /// Данные Аренды
        /// </summary>
        public IEnumerable<MonthBusiness> Data
        {
            get { return m_data; }
            set
            {
                if (m_data != value)
                {
                    m_data = value;
                    if (value != null)
                        ReMapBussinesses(value);
                }
            }
        }

        private IReadOnlyList<MonthHeaderModel> m_Monthes = new List<MonthHeaderModel>();
        /// <summary>
        /// Возвращает или задаёт месяцы
        /// </summary>
        public IReadOnlyList<MonthHeaderModel> Monthes
        {
            get { return m_Monthes; }
            set
            {
                if (m_Monthes != value)
                {
                    m_Monthes = value;

                    if (value != null)
                    {
                        MonthHeaderModel curentML = null;
                        for (int i = 0; i < value.Count; i++)
                        {
                            curentML = value[i];
                            if (curentML != null)
                            {
                                curentML.Previous = (i - 1) >= 0 ? value[i - 1] : null;
                                curentML.Next = (i + 1) < value.Count ? value[i + 1] : null;
                            }
                        }

                        //простановка индекса колонок для Grid'а
                        GridIndexHelper.SetIndexes(value);

                    }

                    if (MonthesChanged != null)
                        MonthesChanged(this);

                    OnPropertyChanged();
                }
            }
        }

        private IReadOnlyList<CarModel> m_CarModels = new List<CarModel>();
        /// <summary>
        /// Возвращает или задаёт набор моделей авто для View
        /// </summary>
        public IReadOnlyList<CarModel> CarModels { get { return m_CarModels; } set { m_CarModels = value; OnPropertyChanged(); } }

        private IReadOnlyList<LeasingElementModel> pv_Leasings = new List<LeasingElementModel>();
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

        private IReadOnlyList<CarComment> m_Comments = new List<CarComment>();
        /// <summary>
        /// Возвращает или задаёт Комментарии к авто
        /// </summary>
        public IReadOnlyList<CarComment> Comments { get { return m_Comments; } set { m_Comments = value; OnPropertyChanged(); } }

        /// <summary>
        /// При изменении наборма месяцев
        /// </summary>
        public event LeasingSetEvent MonthesChanged;

        #region Перевод из одной модели данных в текущую

        /// <summary>
        /// Перевод из старой модели данных в новую (костыль)
        /// </summary>
        /// <param name="businesses">Набор занятости авто из БД или тестовый</param>
        public void ReMapBussinesses(IEnumerable<MonthBusiness> businesses)
        {
            CarModels = GetCarModels(businesses);
            Comments = GetComments(CarModels);

            Monthes = GetMonthes(businesses);
            //!!зависит от заполнения CarModels
            Leasings = GetLeasingModels(businesses);

            DaysCount = Monthes.Sum(m => m.Month.DayCount);
        }

        IReadOnlyList<CarModel> GetCarModels(IEnumerable<MonthBusiness> data)
        {
            var rowIndex = 0;
            return data
                .SelectMany(mb => mb.CarBusiness)
            .Select(cb => cb.Name)
            .Distinct()
            .Select(name => new CarModel() { Text = name, RowIndex = rowIndex++ }).ToList();
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
                    var rowIndex = 0;
                    leasingBarModels.AddRange(item.Business.Select(
                        b =>
                        {
                            rowIndex = car == null ? 0 : car.RowIndex;
                            var model = new LeasingElementModel()
                            {
                                CarName = m_CarModels.Count > 0 ? m_CarModels[rowIndex].Text : b.CarName,
                                Leasing = b,
                                RowIndex = rowIndex,
                                DayColumnWidth = 21d
                            };

                            if (b.CurrentMonth != null)
                                model.Monthes = new MonthHeaderModel[] { new MonthHeaderModel(this) { Month = b.CurrentMonth } };
                            else
                                model.Monthes = this.Monthes.Where(m => b.Monthes.Any(bm => bm.Equals(m.Month))).ToArray();

                            return model;
                        }));
                }
                index++;
            }

            return leasingBarModels;
        }

        IReadOnlyList<MonthHeaderModel> GetMonthes(IEnumerable<MonthBusiness> businesses)
        {
            var monthes = new List<MonthHeaderModel>();

            var first = businesses.First();
            if (businesses.Count() == 1 && first.Monthes != null)
            {
                foreach (var item in first.Monthes)
                {
                    monthes.Add(new MonthHeaderModel(this) { Month = item });
                }
            }
            else
            {
                foreach (var item in businesses)
                {
                    monthes.Add(new MonthHeaderModel(this) { Month = item.Month });
                }
            }

            return monthes;
        }

        public void Dispose()
        {
            if(Monthes != null)
                foreach (var monthHeader in Monthes)
                {
                    monthHeader.Dispose();
                }
        }

        #endregion
    }
}
