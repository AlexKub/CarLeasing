using CarLeasingViewer.Interfaces;
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

        /// <summary>
        /// Изменения в Наборе
        /// </summary>
        /// <param name="n">Ссылка на изменившийся набор</param>
        public LeasingSetEventArgs(LeasingSet n) : this(n, n) { }

        /// <summary>
        /// Изменения в наборе
        /// </summary>
        /// <param name="n">Новый набор</param>
        /// <param name="o">Старый набор</param>
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
    public class LeasingSet : ViewModels.ViewModelBase, IDisposable, IPeriod
    {
        /// <summary>
        /// Базовый набор (без сортировок)
        /// </summary>
        BaseSet m_baseSet;

        /// <summary>
        /// Флаг использования базового набора в текущий момент
        /// </summary>
        bool m_baseSetted = true;

        /// <summary>
        /// Флаг, что текущий набор отсортирован (основная коллекция не полная)
        /// </summary>
        public bool Sorted { get { return !m_baseSetted; } }

        DateTime m_dateStart;
        /// <summary>
        /// Дата начала (при сортировке)
        /// </summary>
        public DateTime DateStart
        {
            get { return m_dateStart; }
            private set
            {
                m_dateStart = value;
                MonthCount = this.CalculateMonthCount();
            }
        }

        DateTime m_dateEnd;
        /// <summary>
        /// Дата окончания (при сортировке)
        /// </summary>
        public DateTime DateEnd
        {
            get { return m_dateEnd; }
            private set
            {
                m_dateEnd = value;
                MonthCount = this.CalculateMonthCount();
            }
        }

        /// <summary>
        /// Количество месяцев в рассматриваемом периоде Набора
        /// </summary>
        public int MonthCount { get; private set; }

        #region Notify properties

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

        private Controls.LeasingChart pv_Chart;
        /// <summary>
        /// Возвращает или задаёт График, к которому принадлежит текущий набор
        /// </summary>
        public Controls.LeasingChart Chart { get { return pv_Chart; } set { if (pv_Chart != value) { pv_Chart = value; OnPropertyChanged(); } } }

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
                        var daysCount = 0;
                        for (int i = 0; i < value.Count; i++)
                        {
                            curentML = value[i];
                            if (curentML != null)
                            {
                                daysCount += curentML?.Month.DayCount ?? 0;
                                curentML.Previous = (i - 1) >= 0 ? value[i - 1] : null;
                                curentML.Next = (i + 1) < value.Count ? value[i + 1] : null;
                            }
                        }

                        DaysCount = daysCount;

                        //простановка индекса колонок для Grid'а
                        GridIndexHelper.SetIndexes(value);

                        SetInsurance();
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
        public IReadOnlyList<CarModel> CarModels
        {
            get { return m_CarModels; }
            set
            {
                m_CarModels = value;

                //проставляем количество строк, на которые будут биндится модели
                RowsCount = value == null ? 0 : value.Count;

                CarsChanged?.Invoke(new LeasingSetEventArgs(this));
                OnPropertyChanged();
            }
        }

        private IReadOnlyList<IDrawableBar> pv_Leasings = new List<IDrawableBar>();
        /// <summary>
        /// Возвращает или задаёт набор занятости автомобилей в текущем месяце
        /// </summary>
        public IReadOnlyList<IDrawableBar> Leasings
        {
            get { return pv_Leasings; }
            set
            {
                if (pv_Leasings != value)
                {
                    pv_Leasings = value;

                    if (value != null)
                    {
                        var distinctRowIndexes = new List<int>(500);
                        foreach (var leasing in value)
                        {
                            if (!distinctRowIndexes.Contains(leasing.RowIndex))
                                distinctRowIndexes.Add(leasing.RowIndex);
                        }
                    }

                    OnPropertyChanged();
                }
            }
        }

        private IReadOnlyList<CarCommentModel> m_Comments = new List<CarCommentModel>();
        /// <summary>
        /// Возвращает или задаёт Комментарии к авто
        /// </summary>
        public IReadOnlyList<CarCommentModel> Comments { get { return m_Comments; } set { m_Comments = value; CommentsChanged?.Invoke(new LeasingSetEventArgs(this)); OnPropertyChanged(); } }

        #endregion

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

        public LeasingSet()
        {

        }

        public LeasingSet(Controls.LeasingChart chart)
        {
            Chart = chart;
        }

        #region Перевод из одной модели данных в текущую

        /// <summary>
        /// Перевод из старой модели данных в новую (костыль)
        /// </summary>
        /// <param name="businesses">Набор занятости авто из БД или тестовый</param>
        public void ReMapBussinesses(IEnumerable<MonthBusiness> businesses)
        {
            var cars = GetCarModels(businesses);
            CarModels = cars;
            var comments = GetComments(CarModels);
            Comments = comments;

            Monthes = GetMonthes(businesses);
            //!!зависит от заполнения CarModels
            var leasings = GetLeasingModels(businesses);
            Leasings = leasings;

            DaysCount = Monthes.Sum(m => m.Month.DayCount);

            m_baseSet = new BaseSet(cars, comments, leasings);
            m_baseSetted = true;
        }

        List<CarModel> GetCarModels(IEnumerable<MonthBusiness> data)
        {
            var rowIndex = 0;
            var notOrdered = data
                .SelectMany(mb => mb.CarBusiness)
            .Select(cb => new { cb.Name, cb.ID, cb.Maintenance, Data = cb })
            .Distinct()
            .Select(o =>
            new CarModel(o.Data)
            {
                Text = o.Name,
                Car = App.Cars.FirstOrDefault(c => c.ID.Equals(o.ID)),
                IsMaintaining = o.Maintenance != null,
            }).ToList();

            var ordered = SortManager.OrderByPrice(notOrdered).ToList();

            foreach (var model in ordered)
                model.RowIndex = rowIndex++;

            return ordered;
        }

        List<CarCommentModel> GetComments(IReadOnlyList<CarModel> cars)
        {
            return cars.Select(car => new CarCommentModel() { RowIndex = car.RowIndex, Comment = (car.Text + "_comment") }).ToList();
        }

        List<IDrawableBar> GetLeasingModels(IEnumerable<MonthBusiness> monthBuisnesses)
        {
            var leasingBarModels = new List<IDrawableBar>();

            var index = 0;
            var currentPeriod = Sorted ? new Period(DateStart, DateEnd) : new Period(Monthes.First().Month.FirstDate, Monthes.Last().Month.LastDate);
            var columnWidth = AppStyles.TotalColumnWidth;
            var insuranceIcon = IconsInfo.InsuranceDay;
            foreach (var business in monthBuisnesses)
            {
                foreach (var item in business.CarBusiness)
                {
                    var car = m_CarModels.FirstOrDefault(c => c.Text.Equals(item.Name));
                    var rowIndex = 0;
                    leasingBarModels.AddRange(item.Leasings.Select(
                        b =>
                        {
                            rowIndex = car == null ? 0 : car.RowIndex;
                            var model = new LeasingBarModel(this)
                            {
                                CarName =
                                    m_CarModels.Count > 0
                                        ? m_CarModels[rowIndex].Text
                                        : b.CarName,
                                Leasing = b,
                                RowIndex = rowIndex,
                                DayColumnWidth = columnWidth
                            };

                            if (b.CurrentMonth != null)
                                model.Monthes = new MonthHeaderModel[] { new MonthHeaderModel(this) { Month = b.CurrentMonth } };
                            else
                                model.Monthes = this.Monthes.Where(m => b.Monthes.Any(bm => bm.Equals(m.Month))).ToArray();

                            return model;
                        }));

                    if(item.Stornos.Count > 0 && App.SearchSettings.DrawStorno)
                    {
                        leasingBarModels.AddRange(item.Stornos.Select(s => new StornoBarModel(this, s) {
                            RowIndex = rowIndex
                        }));
                    }

                    //отрисовка ремонта
                    if (item.Maintenance != null)
                    {
                        leasingBarModels.Add(
                            new MaintenanceBarModel(this, item)
                            {
                                RowIndex = rowIndex,
                            });
                    }
                    if(this.Include(item.OSAGO_END))
                    {
                        var img = new ImageBarModel(this)
                        {
                            RowIndex = rowIndex,
                            Date = item.OSAGO_END,
                            Bitmap = insuranceIcon
                        };

                        img.SetTooltip(item);

                        leasingBarModels.Add(img);
                    }
                }

                index++;
            }

            return leasingBarModels;
        }

        List<MonthHeaderModel> GetMonthes(IEnumerable<MonthBusiness> businesses)
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

        /// <summary>
        /// Сортировка моделей по дате
        /// </summary>
        /// <param name="date">Выбранная дата</param>
        //public void Sort(DateTime date)
        //{
        //    Sort(date.Date, date);
        //}

        /// <summary>
        /// Сортировка моделей по дате
        /// </summary>
        /// <param name="date">Выбранная дата</param>
        public void Sort(IPeriod period)
        {
            if (m_baseSet.Rows == null || m_baseSet.Rows.Count == 0)
                return;

            var sorted = m_baseSet.Rows.SelectFree(period);

            //для даты начала время не важно
            //если машину берут сегодня, то она занята*
            //* кроме случаев, когда берут и сдают в тот же день
            DateStart = period.DateStart.Date;
            DateEnd = period.DateEnd;

            SetInsurance();

            if (sorted.Count == 0)
                SetEmpty();
            else
                SetSorted(sorted);
        }

        /// <summary>
        /// Простановка пустых коллекций
        /// </summary>
        void SetEmpty()
        {
            SetSorted(null);

            DateStart = default(DateTime);
            DateEnd = default(DateTime);
        }

        void SetSorted(IEnumerable<Controls.LeasingChartManagers.RowManager.Row> rows)
        {
            var cars = new List<CarModel>();
            var leasings = new List<IDrawableBar>();
            var comments = new List<CarCommentModel>();

            //ставим флаг, что используются сортированные данные
            // (базовый набор НЕ используется)
            m_baseSetted = false;

            CarModel car = null;
            CarCommentModel comment = null;
            IDrawableBar leasing = null;
            if (rows != null && rows.Count() > 0)
            {
                var rowIndex = 0;
                foreach (var row in rows)
                {
                    if (row.Car != null)
                    {
                        car = row.Car.Clone();
                        car.RowIndex = rowIndex;
                        cars.Add(car);
                    }

                    if (row.Comment != null)
                    {
                        comment = row.Comment.Clone();
                        comment.RowIndex = rowIndex;
                        comments.Add(comment);
                    }

                    if (row.Bars.Count > 0)
                    {
                        foreach (var bar in row.Bars)
                        {
                            if (bar.Model != null)
                            {
                                leasing = bar.Model.Clone();
                                leasing.RowIndex = rowIndex;
                                leasings.Add(leasing);
                            }
                        }
                    }

                    rowIndex++;
                }
            }

            Chart.ClearManagers();

            CarModels = cars;
            Comments = comments;
            Leasings = leasings;

            Chart.Draw();
        }

        void SetInsurance()
        {
            /*
             * простановка видимости Окончания страховки
             * 
             */
            foreach (var carModel in CarModels)
            {
                if (carModel.ItemInfo != null)
                    if (carModel.ItemInfo.OSAGO_END.Year > 2000)
                    {
                        //проверяем наличие окончания страховки в текущем периоде.
                        //есть - выводим оповещение.
                        //ОСАГО встречается чаще
                        bool visible = this.Include(carModel.ItemInfo.OSAGO_END) || this.Include(carModel.ItemInfo.KASKO_END);
                        carModel.InsuranceVisibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                        continue;
                    }

                carModel.InsuranceVisibility = System.Windows.Visibility.Collapsed;
            }
        }

        public void Dispose()
        {
            if (Monthes != null)
                foreach (var monthHeader in Monthes)
                {
                    monthHeader.Dispose();
                }

            if (Chart != null)
                Chart = null;
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

        /// <summary>
        /// Сброс сортировки
        /// </summary>
        public void ResetSorting()
        {
            if (m_baseSet != null && !m_baseSetted)
            {
                CarModels = m_baseSet.Cars;
                Comments = m_baseSet.Comments;
                Leasings = m_baseSet.Leasings;

                m_baseSetted = true;
            }

            DateStart = default(DateTime);
            DateEnd = default(DateTime);

            if (Chart != null)
            {
                Chart.ClearManagers();

                Chart.Draw();
            }
        }

        DateTime IPeriod.DateStart { get { return Sorted ? DateStart : ((Monthes.FirstOrDefault()?.Month?.FirstDate) ?? DateTime.MinValue); } }

        DateTime IPeriod.DateEnd { get { return Sorted ? DateEnd : ((Monthes.LastOrDefault()?.Month?.LastDate) ?? DateTime.MinValue); } }

        /// <summary>
        /// Базовый, не отсортированный набор
        /// </summary>
        [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
        class BaseSet
        {
            public List<CarModel> Cars { get; private set; }

            public List<CarCommentModel> Comments { get; private set; }

            public List<IDrawableBar> Leasings { get; private set; }

            public List<Controls.LeasingChartManagers.RowManager.Row> Rows { get; private set; }

            public BaseSet(List<CarModel> cars, List<CarCommentModel> comments, List<IDrawableBar> leasings)
            {
                Cars = cars;
                Comments = comments;
                Leasings = leasings;

                var rowIndex = 0;
                var rows = new List<Controls.LeasingChartManagers.RowManager.Row>();
                var rowLeasings = new List<LeasingBarModel>();
                foreach (var car in Cars)
                {
                    var newRow = new Controls.LeasingChartManagers.RowManager.Row(rowIndex)
                    {
                        Car = car,
                        Comment = comments[rowIndex]
                    };
                    newRow.AddRange(leasings
                        .Where(l => l.RowIndex == rowIndex)
                        .Select(l => new Controls.LeasingChartManagers.CanvasBarDrawManager.BarData(l)));

                    rows.Add(newRow);
                    rowIndex++;
                }
                Rows = rows;
            }

            string DebugDisplay()
            {
                return "Cars: " + GetStringCount(Cars) + " | Comments: " + GetStringCount(Comments) + " | Leasings: " + GetStringCount(Leasings);
            }

            string GetStringCount<T>(List<T> list)
            {
                return list == null ? "NULL" : list.Count.ToString();
            }
        }
    }
}
