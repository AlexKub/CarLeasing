using RTCManifestGenerator.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Модель для вкладки за Период
    /// </summary>
    public abstract class PeriodTabItem : TabItemModel, IList<MonthBusiness>
    {
        public IEnumerable<Month> AvailableMonthes { get; set; }

        public IEnumerable<int> AvailableYears { get; set; }

        Month m_fromMonth;
        /// <summary>
        /// Возвращает или задаёт Начальный месяц периода
        /// </summary>
        public Month FromMonth
        {
            get { return m_fromMonth; }
            set
            {
                if (m_fromMonth != null && m_fromMonth.Equals(value))
                    return;

                m_fromMonth = value;

                ResetMonthesRange(true);
            }
        }

        Month m_toMonth;
        /// <summary>
        /// Возвращает или задаёт Конечный месяц
        /// </summary>
        public Month ToMonth
        {
            get { return m_toMonth; }
            set
            {
                if (m_toMonth != null && m_toMonth.Equals(value))
                    return;

                m_toMonth = value;

                ResetMonthesRange(false);
            }
        }

        #region Commands

        public ActionCommand ShowCommand { get { return new ActionCommand(Show); } }
        private void Show()
        {

        }

        #endregion

        #region Notify Properties

        private IEnumerable<Month> pv_FromMonthes;
        /// <summary>
        /// Возвращает или задаёт
        /// </summary>
        public IEnumerable<Month> FromMonthes { get { return pv_FromMonthes; } set { if (pv_FromMonthes != value) { pv_FromMonthes = value; OnPropertyChanged(); } } }

        private IEnumerable<Month> pv_ToMonthes;
        /// <summary>
        /// Возвращает или задаёт
        /// </summary>
        public IEnumerable<Month> ToMonthes { get { return pv_ToMonthes; } set { if (pv_ToMonthes != value) { pv_ToMonthes = value; OnPropertyChanged(); } } }

        private IEnumerable<int> pv_FromYears;
        /// <summary>
        /// Возвращает или задаёт
        /// </summary>
        public IEnumerable<int> FromYears { get { return pv_FromYears; } set { if (pv_FromYears != value) { pv_FromYears = value; OnPropertyChanged(); } } }

        private IEnumerable<int> pv_ToYears;
        /// <summary>
        /// Возвращает или задаёт
        /// </summary>
        public IEnumerable<int> ToYears { get { return pv_ToYears; } set { if (pv_ToYears != value) { pv_ToYears = value; OnPropertyChanged(); } } }


        private int pv_FromMonthIndex;
        /// <summary>
        /// Возвращает или задаёт
        /// </summary>
        public int FromMonthIndex
        {
            get { return pv_FromMonthIndex; }
            set
            {
                if (pv_FromMonthIndex != value)
                {
                    pv_FromMonthIndex = value;

                    if (value >= 0 && value < 13)
                        FromMonth = FromMonthes.ElementAt(value);

                    OnPropertyChanged();
                }
            }
        }

        private int pv_FromYear;
        /// <summary>
        /// Возвращает или задаёт Начальный год
        /// </summary>
        public int FromYear { get { return pv_FromYear; } set { if (pv_FromYear != value) { pv_FromYear = value; OnPropertyChanged(); ResetMonthesRange(true); } } }

        private int pv_ToMonthIndex;
        /// <summary>
        /// Возвращает или задаёт
        /// </summary>
        public int ToMonthIndex
        {
            get { return pv_ToMonthIndex; }
            set
            {
                if (pv_ToMonthIndex != value)
                {
                    pv_ToMonthIndex = value;

                    if (value >= 0 && value < 13)
                        ToMonth = ToMonthes.ElementAt(value);

                    OnPropertyChanged();
                }
            }
        }

        private int pv_ToYear;
        /// <summary>
        /// Возвращает или задаёт Конечный год
        /// </summary>
        public int ToYear { get { return pv_ToYear; } set { if (pv_ToYear != value) { pv_ToYear = value; OnPropertyChanged(); ResetMonthesRange(false); } } }

        private ObservableCollection<MonthBusiness> pv_Leasings = new ObservableCollection<MonthBusiness>();
        /// <summary>
        /// Возвращает или задаёт Коллекцию моделей Занятости автомобилей за указанный период
        /// </summary>
        public ObservableCollection<MonthBusiness> Leasings { get { return pv_Leasings; } set { if (pv_Leasings != value) { pv_Leasings = value; OnPropertyChanged(); } } }

        private string pv_ErrorPeriodText;
        /// <summary>
        /// Возвращает или задаёт
        /// </summary>
        public string ErrorPeriodText { get { return pv_ErrorPeriodText; } set { if (pv_ErrorPeriodText != value) { pv_ErrorPeriodText = value; OnPropertyChanged(); } } }

        private bool pv_IsLoading;
        /// <summary>
        /// Возвращает или задаёт
        /// </summary>
        public bool IsLoading { get { return pv_IsLoading; } set { if (pv_IsLoading != value) { pv_IsLoading = value; OnPropertyChanged(); } } }

        

        #endregion

        public PeriodTabItem(string title) : base(title) { }

        void ResetMonthesRange(bool from)
        {
            if(from) //для начального месяца
            {
                if (!App.AvailableMonthes.ContainsKey(ToYear))
                    return;
                //получаем список всех доступных месяцев на выбранный год
                var monthes = App.AvailableMonthes[ToYear];

                //если выбран один год, то исключаем те месяцы, что находятся за выбранным начальным
                ToMonthes = FromYear == ToYear ? monthes.Where(m => m >= FromMonth) : monthes;

                if (ToMonthIndex >= 0)
                    //пересчитываем индекс для выбранного месяца, т.к. изменили коллекцию
                    ToMonthIndex = ToMonthes.IndexOf(ToMonth);
            }
            else //для конечного месяца
            {
                if (!App.AvailableMonthes.ContainsKey(FromYear))
                    return;

                //получаем список всех доступных месяцев на выбранный год
                var monthes = App.AvailableMonthes[FromYear];

                //если выбран один год, то исключаем те месяцы, что находятся за выбранным начальным
                FromMonthes = FromYear == ToYear ? monthes.Where(m => m <= ToMonth) : monthes;

                if (FromMonthIndex >= 0)
                    //пересчитываем индекс для выбранного месяца, т.к. изменили коллекцию
                    FromMonthIndex = FromMonthes.IndexOf(FromMonth);
            }
        }

        public async void AddRange(IEnumerable<MonthBusiness> leasings)
        {
            if (leasings == null || leasings.Count() == 0)
                return;

            IsLoading = true;

            var tasks = new List<Task>();
            foreach (var item in leasings)
                tasks.Add(AddLeasing(item));

            await Task.WhenAll(tasks);

            IsLoading = false;
        }

        public async void Add(MonthBusiness leasing)
        {
            IsLoading = true;

            await AddLeasing(leasing);

            IsLoading = false;
        }

        Task AddLeasing(MonthBusiness leasing)
        {
            return App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => { Leasings.Add(leasing); })).Task;
        }

        #region IList<MonthBusiness>

        public int Count
        {
            get
            {
                return ((IList<MonthBusiness>)pv_Leasings).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<MonthBusiness>)pv_Leasings).IsReadOnly;
            }
        }

        public MonthBusiness this[int index]
        {
            get
            {
                return ((IList<MonthBusiness>)pv_Leasings)[index];
            }

            set
            {
                ((IList<MonthBusiness>)pv_Leasings)[index] = value;
            }
        }

        public int IndexOf(MonthBusiness item)
        {
            return ((IList<MonthBusiness>)pv_Leasings).IndexOf(item);
        }

        public void Insert(int index, MonthBusiness item)
        {
            ((IList<MonthBusiness>)pv_Leasings).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<MonthBusiness>)pv_Leasings).RemoveAt(index);
        }

        public void Clear()
        {
            ((IList<MonthBusiness>)pv_Leasings).Clear();
        }

        public bool Contains(MonthBusiness item)
        {
            return ((IList<MonthBusiness>)pv_Leasings).Contains(item);
        }

        public void CopyTo(MonthBusiness[] array, int arrayIndex)
        {
            ((IList<MonthBusiness>)pv_Leasings).CopyTo(array, arrayIndex);
        }

        public bool Remove(MonthBusiness item)
        {
            return ((IList<MonthBusiness>)pv_Leasings).Remove(item);
        }

        public IEnumerator<MonthBusiness> GetEnumerator()
        {
            return ((IList<MonthBusiness>)pv_Leasings).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<MonthBusiness>)pv_Leasings).GetEnumerator();
        }

        #endregion

    }
}
