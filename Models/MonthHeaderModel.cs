using System;

namespace CarLeasingViewer.Models
{
    public class MonthHeaderModel : ViewModels.ViewModelBase, IIndexable
    {
        private Month pv_Month;
        /// <summary>
        /// Возвращает или задаёт
        /// </summary>
        public Month Month { get { return pv_Month; } set { if (pv_Month != value) { pv_Month = value; OnPropertyChanged(); } } }

        private int pv_ColumnIndex;
        /// <summary>
        /// Возвращает или задаёт Индекс колонки в Grid'е
        /// </summary>
        public int ColumnIndex { get { return pv_ColumnIndex; } set { if (pv_ColumnIndex != value) { pv_ColumnIndex = value; OnPropertyChanged(); } } }

        #region IIndexable

        int IIndexable.Index { get => pv_ColumnIndex; set => pv_ColumnIndex = value; }

        #endregion

    }
}
