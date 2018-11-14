using CarLeasingViewer.Models;
using RTCManifestGenerator.Commands;
using System;
using System.Collections.Generic;

namespace CarLeasingViewer.ViewModels
{
    public class LeasingViewViewModel : ViewModelBase, IDisposable
    {
        Views.MainWindow2 m_Window;

        private Month m_FromMonth;
        /// <summary>
        /// Возвращает или задаёт Месяц начала просмотра
        /// </summary>
        public Month FromMonth { get { return m_FromMonth; } set { m_FromMonth = value; OnPropertyChanged(); } }

        private Month m_ToMonth;
        /// <summary>
        /// Возвращает или задаёт Месяц окончания периода осмотра
        /// </summary>
        public Month ToMonth { get { return m_ToMonth; } set { m_ToMonth = value; OnPropertyChanged(); } }

        /// <summary>
        /// Общие настройки поиска
        /// </summary>
        public SearchSettings SearchSettings { get { return App.SearchSettings; } }

        /// <summary>
        /// Комманда сортировки по периоду
        /// </summary>
        public ActionCommand SortPeriodCommand { get { return new ActionCommand(SortPeriod); } }
        void SortPeriod()
        {
            Update();
        }

        private StatisticModel pv_Statistic;
        /// <summary>
        /// Возвращает или задаёт Статистику внизу
        /// </summary>
        public StatisticModel Statistic { get { return pv_Statistic; } set { if (pv_Statistic != value) { pv_Statistic = value; OnPropertyChanged(); } } }

        public Views.MainWindow2 Window { get { return m_Window; } set { m_Window = value; } }

        public LeasingViewViewModel() { }

        public LeasingViewViewModel(Views.MainWindow2 window)
        {
            m_Window = window;
        }

        #region Obsolet properties

        private IReadOnlyList<CarModel> pv_Cars;
        /// <summary>
        /// Возвращает или задаёт набор машин
        /// </summary>
        public IReadOnlyList<CarModel> Cars { get { return pv_Cars; } set { if (pv_Cars != value) { pv_Cars = value; GridIndexHelper.SetIndexes(value); OnPropertyChanged(); } } }

        private IReadOnlyList<MonthLeasing> pv_MonthLeasings;
        /// <summary>
        /// Возвращает или задаёт Наборы занятости по месяцам
        /// </summary>
        public IReadOnlyList<MonthLeasing> MonthLeasings
        {
            get { return pv_MonthLeasings; }
            set
            {
                if (pv_MonthLeasings != value)
                {
                    pv_MonthLeasings = value;

                    if (value != null)
                    {
                        MonthLeasing curentML = null;
                        for (int i = 0; i < value.Count; i++)
                        {
                            curentML = value[i];
                            if (curentML.MonthHeader != null)
                            {
                                curentML.MonthHeader.Previous = (i - 1) >= 0 ? value[i - 1].MonthHeader : null;
                                curentML.MonthHeader.Next = (i + 1) < value.Count ? value[i + 1].MonthHeader : null;
                            }
                        }

                        //простановка индекса колонок для Grid'а
                        GridIndexHelper.SetIndexes(value);
                    }

                    OnPropertyChanged();
                }
            }
        }

        private IReadOnlyList<CarCommentModel> pv_Comments;
        /// <summary>
        /// Возвращает или задаёт набор Комментариев к машинам
        /// </summary>
        public IReadOnlyList<CarCommentModel> Comments { get { return pv_Comments; } set { if (pv_Comments != value) { pv_Comments = value; GridIndexHelper.SetIndexes(value); OnPropertyChanged(); } } }

        #endregion

        private LeasingSet m_LeasingSet = new LeasingSet();
        /// <summary>
        /// Возвращает или задаёт Набор занаятости Авто
        /// </summary>
        public LeasingSet LeasingSet { get { return m_LeasingSet; } set { m_LeasingSet = value; OnSetChanged(value); OnPropertyChanged(); } }

        void OnSetChanged(LeasingSet set)
        {
            if (set == null)
                return;

            switch(set.Monthes.Count)
            {
                case 0:
                    break;
                case 1:
                    FromMonth = set.Monthes[0].Month;
                    ToMonth = set.Monthes[0].Month;
                    break;
                default:
                    FromMonth = set.Monthes[0].Month;
                    ToMonth = set.Monthes[set.Monthes.Count - 1].Month;
                    break;
            }

            if (m_Window != null)
                m_Window.LeasingChart.LeasingSet = set;

            Statistic = new StatisticModel();
            Statistic.Load(set);
        }

        public void Update()
        {
            if (FromMonth == null || ToMonth == null)
                return;

            var newSet = new LeasingSet() { Data = DataManager.GetDataset(FromMonth, ToMonth) };
            LeasingSet = newSet;

            if (m_Window != null)
            {
                newSet.Chart = m_Window.LeasingChart;
                m_Window.LeasingChart.Draw();
            }
        }

        public void Dispose()
        {
            if (LeasingSet != null)
            {
                LeasingSet.Dispose();
                LeasingSet = null;
            }

            Statistic = null;
        }
    }
}
