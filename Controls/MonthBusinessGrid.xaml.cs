using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CarLeasingViewer.Controls
{
    /// <summary>
    /// Interaction logic for MonthBusinessGrid.xaml
    /// </summary>
    public partial class MonthBusinessGrid : UserControl
    {
        static LogSet m_loger = LogManager.GetDefaultLogSet<MonthBusinessGrid>();

        const string NameSearchFlag = "NameSearch";
        const string BusinessSearchFlag = "BusinessSearch";

        IEnumerable<CarBusiness> m_baseCollection;
        Task<IEnumerable<CarBusiness>> m_pausedSearchTask;
        List<int> m_selectingDays = new List<int>();
        int m_selectingStartDay = 0;
        Dictionary<string, CancellationTokenSource> m_searchTasks = new Dictionary<string, CancellationTokenSource>();

        #region Dependency properties

        public static readonly DependencyProperty dp_MonthBusinessProperty = DependencyProperty.Register(nameof(MonthBusiness), typeof(MonthBusiness), typeof(MonthBusinessGrid), new FrameworkPropertyMetadata() { DefaultValue = default(MonthBusiness) });
        public MonthBusiness MonthBusiness { get { return (MonthBusiness)GetValue(dp_MonthBusinessProperty); } set { SetValue(dp_MonthBusinessProperty, value); } }

        public static readonly DependencyProperty dp_StartDayProperty = DependencyProperty.Register(nameof(StartDay), typeof(DayOfWeek), typeof(MonthBusinessGrid), new FrameworkPropertyMetadata() { DefaultValue = default(DayOfWeek) });
        public DayOfWeek StartDay { get { return (DayOfWeek)GetValue(dp_StartDayProperty); } set { SetValue(dp_StartDayProperty, value); } }

        public static readonly DependencyProperty dp_DayCountProperty = DependencyProperty.Register(nameof(DayCount), typeof(int), typeof(MonthBusinessGrid), new FrameworkPropertyMetadata() { DefaultValue = default(int) });
        public int DayCount { get { return (int)GetValue(dp_DayCountProperty); } set { SetValue(dp_DayCountProperty, value); } }

        public static readonly DependencyProperty dp_DayWidthProperty = DependencyProperty.Register(nameof(DayWidth), typeof(double), typeof(MonthBusinessGrid), new FrameworkPropertyMetadata() { DefaultValue = default(double) });
        public double DayWidth { get { return (double)GetValue(dp_DayWidthProperty); } set { SetValue(dp_DayWidthProperty, value); } }

        public static readonly DependencyProperty dp_MonthProperty = DependencyProperty.Register(nameof(Month), typeof(Month), typeof(MonthBusinessGrid), new FrameworkPropertyMetadata() { DefaultValue = default(Month) });
        public Month Month { get { return (Month)GetValue(dp_MonthProperty); } set { SetValue(dp_MonthProperty, value); } }

        public static DependencyProperty dp_PauseSorting = DependencyProperty.Register(nameof(PauseSorting), typeof(bool), typeof(MonthBusinessGrid),
            new FrameworkPropertyMetadata()
            {
                DefaultValue = false
            ,
                PropertyChangedCallback = (s, e) =>
                {
                    var _this = s as MonthBusinessGrid;
                    var resumed = !(bool)e.NewValue;

                    if (resumed)
                    {
                        if (_this.m_pausedSearchTask != null)
                            _this.Sort(_this.m_pausedSearchTask);
                    }
                }
            });
        public bool PauseSorting { get { return (bool)GetValue(dp_PauseSorting); } set { SetValue(dp_PauseSorting, value); } }

        public static DependencyProperty dp_TitleSearch = DependencyProperty.Register(nameof(TitleSearch), typeof(string), typeof(MonthBusinessGrid), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(string)
            ,
            PropertyChangedCallback = (s, e) =>
            {
                (s as MonthBusinessGrid).OnTitleSearchChanged(e.NewValue as string);
            }
        });
        public string TitleSearch { get { return (string)GetValue(dp_TitleSearch); } set { SetValue(dp_TitleSearch, value); } }

        public static DependencyProperty dp_ContextSearch = DependencyProperty.Register(nameof(ContextSearch), typeof(string), typeof(MonthBusinessGrid), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(string)
            ,
            PropertyChangedCallback = (s, e) =>
            {
                (s as MonthBusinessGrid).OnBusinessSearchChanged(e.NewValue as string);
            }
        });
        public string ContextSearch { get { return (string)GetValue(dp_ContextSearch); } set { SetValue(dp_ContextSearch, value); } }

        public static DependencyProperty dp_NameSearchText = DependencyProperty.Register(nameof(NameSearchText), typeof(string), typeof(MonthBusinessGrid), new FrameworkPropertyMetadata() { DefaultValue = default(string) });
        public string NameSearchText { get { return (string)GetValue(dp_NameSearchText); } set { SetValue(dp_NameSearchText, value); } }

        public static DependencyProperty dp_IsLoading = DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(MonthBusinessGrid), new FrameworkPropertyMetadata() { DefaultValue = default(bool) });
        public bool IsLoading { get { return (bool)GetValue(dp_IsLoading); } set { SetValue(dp_IsLoading, value); } }

        public static DependencyProperty dp_SelectedDays = DependencyProperty.Register(nameof(SelectedDays), typeof(IEnumerable<Day>), typeof(MonthBusinessGrid), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(IEnumerable<Day>)
            ,
            PropertyChangedCallback = (s, e) => { (s as MonthBusinessGrid).DateSorting(e.NewValue as IEnumerable<Day>); }
        });

        public IEnumerable<Day> SelectedDays { get { return (IEnumerable<Day>)GetValue(dp_SelectedDays); } set { SetValue(dp_SelectedDays, value); } }

        /// <summary>
        /// Ширина контрола с днями месяца
        /// </summary>
        public static DependencyProperty dp_RightColumnWidth = DependencyProperty.Register(nameof(RightColumnWidth), typeof(double), typeof(MonthBusinessGrid), new FrameworkPropertyMetadata() { DefaultValue = default(double) });
        public double RightColumnWidth { get { return (double)GetValue(dp_RightColumnWidth); } set { SetValue(dp_RightColumnWidth, value); } }

        #endregion

        public MonthBusinessGrid()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == DataContextProperty)
            {
                var context = DataContext as MonthBusiness;

                if (context != null)
                    m_baseCollection = context.BaseCollection;
                else
                    m_baseCollection = Enumerable.Empty<CarBusiness>();
            }
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            var s = sender as ItemsControl;

            if (s == null)
                return;

            s.ToolTip = s.ActualWidth.ToString() + " | " + s.Width.ToString();

            MessageBox.Show(s.ActualWidth.ToString() + " | " + s.Width.ToString());
        }

        private void Day_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var day = GetDay(sender);

            if (day == null)
                return;

            var set = GetSet();

            if (set == null)
                return;

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    {
                        //ничего не делаем, если зажат Shift/Ctrl - идёт мультиселект
                        bool keyPressed = ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                                || (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

                        if (m_selectingStartDay == 0)
                        {
                            HighLightDay(day, false, keyPressed);

                            ResetSorting();
                        }
                        else
                        {
                            var m = GetMonth();
                            if (IsMultiselecting())
                                set.Sort(new DateTime(m.Year, m.Index, m_selectingDays.Min()), new DateTime(m.Year, m.Index, m_selectingDays.Max()));
                            else
                                set.Sort(new DateTime(m.Year, m.Index, day.Index));
                        }
                    }
                    break;
                default:
                    break;
            }

            //todo : select
        }

        private void Day_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var day = GetDay(sender);

            if (day == null)
                return;

            if (e.ChangedButton == MouseButton.Left)
            {
                m_selectingStartDay = m_selectingStartDay == day.Index ? 0 : day.Index;

                //изменяем подсветку для Дня при клике
                //  кликнули первый раз - подсветили
                //  кликнули повторно - убрали
                //if (!HasHightlighted(day))
                //{
                bool isMultyselecting = ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                || (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

                HighLightDay(day, true, isMultyselecting);

                NameSearchText = string.Empty;

                Keyboard.ClearFocus();

                //}
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                DownlightDays();

                ResetSorting();
            }
        }

        void SelectDay(Day day, bool select = true)
        {
            var set = GetSet();
            if (set != null)
            {
                if (select)
                {
                    var m = GetMonth();
                    set.Sort(new DateTime(m.Year, m.Index, day.Index));
                }
                else
                    ResetSorting();
            }
        }

        private void Day_MouseEnter(object sender, MouseEventArgs e)
        {
            var day = GetDay(sender);

            if (day == null)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (HasHightlighted(day))
                    DownlightDays(multiselect: true);
                else
                    HighLightDay(day, true, true);


                //HighLightDay(day, true);
                //m_selectingDays.Add(day);
            }
        }

        Day GetDay(object sender)
        {
            var tb = sender as TextBlock;

            if (tb == null)
                return null;

            return tb.DataContext as Day;
        }

        LeasingSet GetSet()
        {
            return (DataContext as MonthHeaderModel)?.OwnerSet;
        }

        Month GetMonth()
        {
            var context = DataContext as MonthBusiness;
            return context?.Month;
        }

        void HighLightDay(Day day, bool hightlight, bool multiselect = false)
        {
            day.Selected = hightlight;

            if (!multiselect)
                m_selectingDays.Clear();

            if (hightlight)
                m_selectingDays.Add(day.Index);
            else if (multiselect)
                m_selectingDays.Remove(day.Index);

            DownlightDays(day, multiselect);
        }

        void DownlightDays(Day day = null, bool multiselect = false)
        {
            if (day == null && !multiselect)
            {
                m_selectingDays.Clear();
                multiselect = true;
            }

            var month = GetMonth();

            if (month != null)
            {
                //убираем подсветку для остальных дней

                if (multiselect)
                {
                    foreach (var otherDay in month.Days)
                    {
                        if (m_selectingDays.Contains(otherDay.Index))
                            continue;
                        else
                        {
                            if (otherDay.Selected)
                            {
                                otherDay.Selected = false;
                                m_selectingDays.Remove(otherDay.Index);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var otherDay in month.Days)
                    {
                        if (otherDay.Index == day.Index)
                            continue;
                        else
                        {
                            if (otherDay.Selected)
                            {
                                otherDay.Selected = false;
                                m_selectingDays.Remove(otherDay.Index);
                            }
                        }
                    }
                }
            }
        }

        bool HasHightlighted(Day day)
        {
            if (day == null)
                return false;

            return day.Selected;
        }

        bool IsMultiselecting() { return m_selectingDays.Count > 1; }

        private void OnTitleSearchChanged(string text)
        {
            Search(NameSearchFlag, text, () =>
            {
                var baseCollection = (DataContext as MonthBusiness).BaseCollection;

                string lowerName = text.ToLower();

                return baseCollection.Where(cb => cb.Name != null && cb.Name.ToLower().Contains(lowerName));
            });
        }

        /// <summary>
        /// При изменении поиска по съёмщикам
        /// </summary>
        /// <param name="text"></param>
        private void OnBusinessSearchChanged(string text)
        {
            Search(BusinessSearchFlag, text, () =>
            {
                IEnumerable<CarBusiness> baseCollection = null;

                Dispatcher.Invoke(() => baseCollection = (DataContext as MonthBusiness).BaseCollection);

                string lowerCase = text.ToLower();

                return baseCollection.Where(cb => cb.Business.Any(b => b.Title.ToLower().Contains(lowerCase)));
            });
        }

        void Search(string searchFlag, string text, Func<IEnumerable<CarBusiness>> search)
        {
            CancellationTokenSource oldTask = null;

            if (m_searchTasks.ContainsKey(searchFlag))
                oldTask = m_searchTasks[searchFlag];

            if (oldTask != null)
            {
                CancelTask(searchFlag);
                m_searchTasks[searchFlag] = null;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                ResetSorting();
                return;
            }

            var context = DataContext as MonthBusiness;

            if (context == null)
                return;

            if (context.BaseCollectionCount > 0)
            {
                var ct = new CancellationTokenSource();

                m_searchTasks[searchFlag] = ct; //сохраняем, чтобы можно было отменить из последующих

                var baseCollection = context.BaseCollection;

                //если всё-таки начали искать, снимаем фильтр дней
                DownlightDays();

                //сортировка с отсрочкой при наборе
                //чтобы не запускать по таску на каждую букву, ждём чуть-чуть
                var sortTask = new Task<IEnumerable<CarBusiness>>(() =>
                {
                    //int waitMS = 900;
                    //while (waitMS > 0)
                    //{
                    //    Thread.Sleep(100);
                    //    waitMS -= 100;
                    //    if (ct.IsCancellationRequested)
                    //        break;
                    //}

                    if (ct.IsCancellationRequested)
                        return baseCollection;

                    if (string.IsNullOrEmpty(text))
                        return baseCollection;

                    string lowerName = text.ToLower();

                    return search.Invoke();
                }, ct.Token);

                Sort(sortTask, ct);
            }
        }

        void CancelTask(string taskFlag)
        {
            var task = m_searchTasks[taskFlag];

            if (task != null)
                task.Cancel();
        }

        void DateSorting(IEnumerable<Day> days)
        {

            if (days == null)
                ResetSorting();

            var count = days.Count();

            var m = GetMonth();

            switch (count)
            {
                case 0:
                    ResetSorting();
                    break;
                case 1:
                    var set = GetSet();
                    if(set != null)
                        set.Sort(new DateTime(m.Year, m.Index, days.First().Index));
                    break;
                default:
                    set = GetSet();
                    var dayIndexes = days.Select(d => d.Index);
                    if (set != null)
                        set.Sort(new DateTime(m.Year, m.Index, dayIndexes.Min())
                        , new DateTime(m.Year, m.Index, dayIndexes.Max()));
                    break;
            }
        }

        /// <summary>
        /// Контроль трудозатратной сортировки
        /// </summary>
        /// <param name="sortTask">Задача сортировки</param>
        /// <param name="ct">Токен отмены (если есть)</param>
        async void Sort(Task<IEnumerable<CarBusiness>> sortTask, CancellationTokenSource ct = null)
        {
            if (PauseSorting) //если поставили на паузу (окно не активно)
            {
                m_pausedSearchTask = sortTask; //заменяем текущий таск на паузе

                return;
            }
            else
            {
                if (ct != null)
                {
                    if (ct.IsCancellationRequested)
                        return; //ничего не делаем, если задание отменено
                }

                var context = DataContext as MonthBusiness;

                if (context != null) //нет контекста - нет нужды сортить, т.к. некуда вставлять результат
                {
                    if (sortTask.Status == TaskStatus.Created)
                        sortTask.Start();

                    try
                    {
                        IsLoading = true;

                        var result = await sortTask;

                        context.CarBusiness.Clear();

                        var tasks = new List<Task>();
                        int itemCounter = 0;
                        var itemsPack = new List<CarBusiness>();
                        var itemsCount = result.Count();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                        //tasks.Add(
                        //Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.DataBind, new Action(() => {
                        foreach (var item in result)
                        {
                            tasks.Add(Dispatcher.BeginInvoke(new Action<CarBusiness>((pi) => context.CarBusiness.Add(pi)), item).Task);

                            //itemCounter++;
                            ////вставляем по одному, чтобы юзерфрендли
                            //itemsPack.Add(item);

                            //if (itemCounter != itemsCount)
                            //    continue;

                            //if (itemsPack.Count == 10 || itemCounter == itemsCount)
                            //{
                            //    foreach (CarBusiness packItem in itemsPack)
                            //        tasks.Add(Dispatcher.BeginInvoke(new Action<CarBusiness>((pi) => context.CarBusiness.Add(pi))).Task);

                            //    itemsPack.Clear();


                            //}
                        }
                        //})).Task);

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        context.CurentCount = result.Count();

                        await Task.WhenAll(tasks);
                    }
                    catch (Exception ex)
                    {
                        m_loger.Log("Возникло исключение при сортировке", ex);
                    }

                    IsLoading = false;
                }
            }
        }

        /// <summary>
        /// Сброс сортировки
        /// </summary>
        void ResetSorting()
        {
            var set = GetSet();

            if (set != null)
                set.ResetSorting();
            //Sort(new Task<IEnumerable<CarBusiness>>(() => m_baseCollection));
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
