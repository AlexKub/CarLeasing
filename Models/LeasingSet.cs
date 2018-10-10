using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Событие набора Занятости авто
    /// </summary>
    /// <param name="set">Набор, в котором произошло изменение</param>
    public delegate void LeasingSetEvent(LeasingSetEventArgs e);

    /// <summary>
    /// Аргументы события Набора логирования
    /// </summary>
    public class LeasingSetEventArgs
    {
        /// <summary>
        /// Старый набор
        /// </summary>
        public LeasingSet Old { get; private set; }
        /// <summary>
        /// Новый набор
        /// </summary>
        public LeasingSet New { get; private set; }

        public LeasingSetEventArgs(LeasingSet n) : this(n, n) { }

        public LeasingSetEventArgs(LeasingSet n, LeasingSet o)
        {
            New = n;
            Old = o;
        }
    }

    /// <summary>
    /// Общий набор Аренд авто для LeasingChart
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
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
                        MonthesChanged(new LeasingSetEventArgs(this));

                    OnPropertyChanged();
                }
            }
        }

        private IReadOnlyList<CarModel> m_CarModels = new List<CarModel>();
        /// <summary>
        /// Возвращает или задаёт набор моделей авто для View
        /// </summary>
        public IReadOnlyList<CarModel> CarModels { get { return m_CarModels; } set { m_CarModels = value; CarsChanged?.Invoke(new LeasingSetEventArgs(this)); OnPropertyChanged(); } }

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

        private IReadOnlyList<CarCommentModel> m_Comments = new List<CarCommentModel>();
        /// <summary>
        /// Возвращает или задаёт Комментарии к авто
        /// </summary>
        public IReadOnlyList<CarCommentModel> Comments { get { return m_Comments; } set { m_Comments = value; CommentsChanged?.Invoke(new LeasingSetEventArgs(this)); OnPropertyChanged(); } }

        /// <summary>
        /// При изменении набора месяцев
        /// </summary>
        public event LeasingSetEvent MonthesChanged;
        /// <summary>
        /// При изменении набора Машин
        /// </summary>
        public event LeasingSetEvent CarsChanged;
        /// <summary>
        /// При изменении набора Комментариев
        /// </summary>
        public event LeasingSetEvent CommentsChanged;

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

        IReadOnlyList<CarCommentModel> GetComments(IReadOnlyList<CarModel> cars)
        {
            return cars.Select(car => new CarCommentModel() { RowIndex = car.RowIndex, Comment = (car.Text + "_comment") }).ToList();
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

        string DebugDisplay()
        {
            if (Monthes == null || Monthes.Count() == 0)
                return "NO MONTHES IN SET";

            //копипаста из BussinessDateConverter (старая версия)
            StringBuilder sb = new StringBuilder();
            //<действие> c XX по ХХ <месяц>

            if (Monthes.Count() == 1)
            {
                var m = Monthes.First().Month;

                if (m == null)
                    return "NULL MONTH";

                sb.Append(m.Name).Append(" ").Append(m.Year.ToString());
            }
            else
            {
                var m1 = Monthes.First().Month;
                var mn = Monthes.Last().Month;

                sb.Append("С ");
                if (m1 == null)
                    sb.Append("NULL");
                else
                    sb.Append(m1.Name).Append(" ").Append(m1.Year.ToString());

                sb.Append(" по ");

                if (mn == null)
                    sb.Append("NULL");
                else
                    sb.Append(mn.Name).Append(" ").Append(mn.Year.ToString());
            }

            return sb.ToString();
        }
    }
}
