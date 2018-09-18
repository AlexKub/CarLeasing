using System;
using System.Collections.Generic;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Аренды одной машины
    /// </summary>
    public class CarLeasingModel : ViewModels.ViewModelBase, IIndexable
    {
        /*
            модель для строки с арендами
        */

        private int pv_RowIndex;
        /// <summary>
        /// Возвращает или задаёт Индекс строки в сетке
        /// </summary>
        public int RowIndex { get { return pv_RowIndex; } set { if (pv_RowIndex != value) { pv_RowIndex = value; OnPropertyChanged(); } } }

        private IReadOnlyList<LeasingBarModel> pv_Leasing;
        /// <summary>
        /// Возвращает или задаёт Периоды аренды
        /// </summary>
        public IReadOnlyList<LeasingBarModel> Leasing { get { return pv_Leasing; } set { if (pv_Leasing != value) { pv_Leasing = value; SetCurrentIndex(value); OnPropertyChanged(); } } }

        public void SetIndex(int index)
        {
            pv_RowIndex = index;
        }

        void SetCurrentIndex(IEnumerable<LeasingBarModel> leasings)
        {
            if (leasings == null)
                return;

            foreach (var item in leasings)
                item.SetIndex(pv_RowIndex);
        }
    }
}
