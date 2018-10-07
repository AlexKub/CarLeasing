using CarLeasingViewer.Models;
using System.Collections.Generic;

namespace CarLeasingViewer.ViewModels
{
    public class LeasingViewViewModel : ViewModelBase
    {
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

        private LeasingSet m_LeasingSet;
        /// <summary>
        /// Возвращает или задаёт Набор занаятости Авто
        /// </summary>
        public LeasingSet LeasingSet { get { return m_LeasingSet; } set { m_LeasingSet = value; OnPropertyChanged(); } }

    }
}
