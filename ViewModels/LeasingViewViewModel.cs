using CarLeasingViewer.Models;
using RTCManifestGenerator.Commands;
using System.Collections.Generic;

namespace CarLeasingViewer.ViewModels
{
    public class LeasingViewViewModel : ViewModelBase
    {
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
        /// Комманда сортировки по периоду
        /// </summary>
        public ActionCommand SortPeriodCommand { get { return new ActionCommand(SortPeriod); } }
        void SortPeriod()
        {
            var newSet = new LeasingSet() { Data = new MonthBusiness[] { DB_Manager.Default.GetBusinessByMonthes(FromMonth, ToMonth) } };
            LeasingSet = newSet;
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

        private IReadOnlyList<CarComment> pv_Comments;
        /// <summary>
        /// Возвращает или задаёт набор Комментариев к машинам
        /// </summary>
        public IReadOnlyList<CarComment> Comments { get { return pv_Comments; } set { if (pv_Comments != value) { pv_Comments = value; GridIndexHelper.SetIndexes(value); OnPropertyChanged(); } } }

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
        }
    }
}
