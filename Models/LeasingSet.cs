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
    public class LeasingSet : ViewModels.ViewModelBase, IDisposable
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
        /// Контекст фильтрации
        /// </summary>
        readonly SelectingService m_selectingContext = new SelectingService();

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
        /// <summary>
        /// Контекст фильтрации
        /// </summary>
        public SelectingService SelectingContext { get => m_selectingContext; }

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
                        var distinctRowIndexes = new List<int>(500);
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

        #region ctors

        /// <summary>
        /// Общий набор Аренд авто для LeasingChart
        /// </summary>
        public LeasingSet()
        {
            SubscribeSelectingContext(true, m_selectingContext);
        }

        /// <summary>
        /// Общий набор Аренд авто для LeasingChart
        /// </summary>
        public LeasingSet(Controls.LeasingChart chart) : this()
        {
            Chart = chart;
        }

        #endregion

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
            return data
                .SelectMany(mb => mb.CarBusiness)
            .Select(cb => cb.Name)
            .Distinct()
            .Select(name => new CarModel() { Text = name, RowIndex = rowIndex++ }).ToList();
        }

        List<CarCommentModel> GetComments(IReadOnlyList<CarModel> cars)
        {
            return cars.Select(car => new CarCommentModel() { RowIndex = car.RowIndex, Comment = (car.Text + "_comment") }).ToList();
        }

        List<LeasingElementModel> GetLeasingModels(IEnumerable<MonthBusiness> monthBuisnesses)
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
        public void Sort(DateTime date)
        {
            var rows = m_baseSet.Rows;

            if (rows != null && rows.Count() > 0)
            {
                var sorted = new List<Controls.LeasingChartManagers.RowManager.Row>();
                foreach (var row in rows)
                {
                    if (row.Bars.Count > 0)
                    {
                        foreach (var bar in row.Bars)
                        {
                            if (bar.Model == null || bar.Model.Leasing == null)
                                continue;

                            if (bar.Model.Leasing.Include(date))
                            {
                                sorted.Add(row);
                                break;
                            }
                        }
                    }
                }

                if (sorted.Count() == 0)
                    SetEmpty();
                else
                {
                    SetSorted(sorted);
                }
            }
        }

        /// <summary>
        /// Сортировка моделей по дате
        /// </summary>
        /// <param name="date">Выбранная дата</param>
        public void Sort(DateTime dateStart, DateTime dateEnd)
        {
            var rows = m_baseSet.Rows;

            if (rows != null && rows.Count() > 0)
            {
                var sorted = new List<Controls.LeasingChartManagers.RowManager.Row>();
                foreach (var row in rows)
                {
                    if (row.Bars.Count > 0)
                    {
                        foreach (var bar in row.Bars)
                        {
                            if (bar.Model == null || bar.Model.Leasing == null)
                                continue;

                            if (bar.Model.Leasing.DateStart > dateEnd)
                                break;

                            if (bar.Model.Leasing.Cross(dateStart, dateEnd))
                            {
                                sorted.Add(row);
                                break;
                            }
                        }
                    }
                }

                if (sorted.Count() == 0)
                    SetEmpty();
                else
                    SetSorted(sorted);
            }
        }

        /// <summary>
        /// Простановка пустых коллекций
        /// </summary>
        void SetEmpty()
        {
            SetSorted(null);
        }

        void SetSorted(IEnumerable<Controls.LeasingChartManagers.RowManager.Row> rows)
        {
            var cars = new List<CarModel>();
            var leasings = new List<LeasingElementModel>();
            var comments = new List<CarCommentModel>();

            //ставим флаг, что используются сортированные данные
            // (базовый набор НЕ используется)
            m_baseSetted = false;

            CarModel car = null;
            CarCommentModel comment = null;
            LeasingElementModel leasing = null;
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

        #endregion

        public void Dispose()
        {
            SubscribeSelectingContext(false, m_selectingContext);

            if (Monthes != null)
            {
                foreach (var monthHeader in Monthes)
                {
                    monthHeader.Dispose();
                }
            }

            if (Chart != null)
                Chart = null;
        }

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
                Chart.ClearManagers();

                CarModels = m_baseSet.Cars;
                Comments = m_baseSet.Comments;
                Leasings = m_baseSet.Leasings;

                if (Chart != null)
                    Chart.Draw();

                m_baseSetted = true;
            }
        }

        void SubscribeSelectingContext(bool subscribe, SelectingService context)
        {
            if (subscribe)
            {
                context.OnSelectionFinished += OnSelectionFinished;
            }
            else
            {
                context.OnSelectionFinished -= OnSelectionFinished;
            }
        }

        void OnSelectionFinished(SelectingService context)
        {
            if (context.IsEmpty)
                ResetSorting();
            else
            {
                var selected = context.SelectedDays;

                if (selected.Count == 0)
                    ResetSorting();
                else if (selected.Count == 1)
                {
                    Sort(selected.First().Date);
                }
                else
                {
                    Sort(selected.First().Date, selected.Last().Date);
                }
            }
        }

        /// <summary>
        /// Базовый, не отсортированный набор
        /// </summary>
        [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
        class BaseSet
        {
            public List<CarModel> Cars { get; private set; }

            public List<CarCommentModel> Comments { get; private set; }

            public List<LeasingElementModel> Leasings { get; private set; }

            public List<Controls.LeasingChartManagers.RowManager.Row> Rows { get; private set; }

            public BaseSet(List<CarModel> cars, List<CarCommentModel> comments, List<LeasingElementModel> leasings)
            {
                Cars = cars;
                Comments = comments;
                Leasings = leasings;

                var rowIndex = 0;
                var rows = new List<Controls.LeasingChartManagers.RowManager.Row>();
                var rowLeasings = new List<LeasingElementModel>();
                foreach (var car in Cars)
                {
                    var newRow = new Controls.LeasingChartManagers.RowManager.Row(rowIndex)
                    {
                        Car = car,
                        Comment = comments[rowIndex]
                    };
                    newRow.AddRange(leasings.Where(l => l.RowIndex == rowIndex).Select(l => new Controls.LeasingChartManagers.CanvasBarDrawManager.BarData(l)));

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
