using System.Linq;
using System.Collections.Generic;

namespace CarLeasingViewer.Models
{
    public class MonthLeasing : ViewModels.ViewModelBase, IIndexable
    {
        private int pv_ColumnIndex;
        /// <summary>
        /// Возвращает или задаёт Индекс колонки в гриде
        /// </summary>
        public int ColumnIndex { get { return pv_ColumnIndex; } set { if (pv_ColumnIndex != value) { pv_ColumnIndex = value; OnPropertyChanged(); } } }

        private MonthHeaderModel pv_MonthHeader;
        /// <summary>
        /// Возвращает или задаёт модель для шапки Месяца
        /// </summary>
        public MonthHeaderModel MonthHeader { get { return pv_MonthHeader; } set { if (pv_MonthHeader != value) { pv_MonthHeader = value; OnPropertyChanged(); } } }

        private IReadOnlyList<LeasingBarModel> pv_Leasings;
        /// <summary>
        /// Возвращает или задаёт набор занятости автомобилей в текущем месяце
        /// </summary>
        public IReadOnlyList<LeasingBarModel> Leasings
        {
            get { return pv_Leasings; }
            set
            {
                if (pv_Leasings != value)
                {
                    pv_Leasings = value;
                    //GridIndexHelper.SetIndexes(value);

                    //проставляем количество строк, на которые будут биндится модели
                    var rowCount = value == null ? 0 : value.Select(i => i.RowIndex).Distinct().Count();

                    RowsCount = rowCount;

                    OnPropertyChanged();
                }
            }
        }

        private int pv_RowsCount;
        /// <summary>
        /// Возвращает или задаёт Количество строк в сетке Занятости авто (количество авто)
        /// </summary>
        public int RowsCount { get { return pv_RowsCount; } private set { if (pv_RowsCount != value) { pv_RowsCount = value; OnPropertyChanged(); } } }

        #region IIndexable

        int IIndexable.Index { get => ColumnIndex; set => ColumnIndex = value; }

        #endregion
    }
}
