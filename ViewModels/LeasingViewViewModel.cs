using System.Collections.Generic;
using CarLeasingViewer.Models;

namespace CarLeasingViewer.ViewModels
{
    public class LeasingViewViewModel : ViewModelBase
    {
        private IReadOnlyList<CarHeaderModel> pv_Cars;
        /// <summary>
        /// Возвращает или задаёт набор машин
        /// </summary>
        public IReadOnlyList<CarHeaderModel> Cars { get { return pv_Cars; } set { if (pv_Cars != value) { pv_Cars = value; GridIndexHelper.SetIndexes(value); OnPropertyChanged(); } } }

        private IReadOnlyList<MonthLeasing> pv_MonthLeasings;
        /// <summary>
        /// Возвращает или задаёт Наборы занятости по месяцам
        /// </summary>
        public IReadOnlyList<MonthLeasing> MonthLeasings { get { return pv_MonthLeasings; } set { if (pv_MonthLeasings != value) { pv_MonthLeasings = value; GridIndexHelper.SetIndexes(value); OnPropertyChanged(); } } }

        private IReadOnlyList<CarComment> pv_Comments;
        /// <summary>
        /// Возвращает или задаёт набор Комментариев к машинам
        /// </summary>
        public IReadOnlyList<CarComment> Comments { get { return pv_Comments; } set { if (pv_Comments != value) { pv_Comments = value; GridIndexHelper.SetIndexes(value); OnPropertyChanged(); } } }

    }
}
