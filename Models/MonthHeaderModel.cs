using System;
using System.Linq;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Модель для месяца на LeasingChart
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class MonthHeaderModel : ViewModels.ViewModelBase, IIndexable, IDisposable
    {
        /// <summary>
        /// Set, к которому принадлежит данная модель
        /// </summary>
        public LeasingSet OwnerSet { get; set; }

        private Month pv_Month;
        /// <summary>
        /// Возвращает или задаёт значение Месяца для текущей обёртки
        /// </summary>
        public Month Month { get { return pv_Month; } set { if (pv_Month != value) { pv_Month = value; SetNextPrevios(); OnPropertyChanged(); } } }

        private int pv_ColumnIndex;
        /// <summary>
        /// Возвращает или задаёт Индекс колонки в Grid'е
        /// </summary>
        public int ColumnIndex { get { return pv_ColumnIndex; } set { if (pv_ColumnIndex != value) { pv_ColumnIndex = value; OnPropertyChanged(); } } }

        private MonthHeaderModel pv_Next;
        /// <summary>
        /// Доступ к модели следующего месяца
        /// </summary>
        public MonthHeaderModel Next { get { return pv_Next; } set { if (pv_Next != value) { pv_Next = value; OnPropertyChanged(); } } }

        private MonthHeaderModel pv_Previous;
        /// <summary>
        /// Доступ к модели предыдущего месяца
        /// </summary>
        public MonthHeaderModel Previous { get { return pv_Previous; } set { if (pv_Previous != value) { pv_Previous = value; OnPropertyChanged(); } } }

        public MonthHeaderModel(LeasingSet set)
        {
            OwnerSet = set;

            Subscribe(true);
        }

        void SetNextPrevios()
        {
            if (OwnerSet == null || OwnerSet.Monthes == null || Month == null)
                return;

            var next = Month.Next();
            Next = OwnerSet.Monthes.FirstOrDefault(mh => mh.Month.Equals(next));

            var previos = Month.Previos();
            Previous = OwnerSet.Monthes.FirstOrDefault(mh => mh.Month.Equals(previos));
        }

        void Subscribe(bool subscribe)
        {
            if(subscribe)
            {
                OwnerSet.MonthesChanged += OwnerSet_MonthesChanged;
            }
            else
            {
                OwnerSet.MonthesChanged -= OwnerSet_MonthesChanged;
            }
        }

        private void OwnerSet_MonthesChanged(LeasingSetEventArgs e)
        {
            SetNextPrevios();
        }

        #region IIndexable

        int IIndexable.Index { get => pv_ColumnIndex; set => pv_ColumnIndex = value; }

        #endregion

        string DebugDisplay()
        {
            return Month == null ? "NO MONTH" : (Month.Name + " " + Month.Year.ToString());
        }

        public void Dispose()
        {
            if(OwnerSet != null)
            {
                Subscribe(false);

                OwnerSet = null;
            }
        }
    }
}
