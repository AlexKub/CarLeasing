using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Аренды одной машины
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("DebugDisplay()")]
    public class LeasingElementsCollectionModel : ViewModels.ViewModelBase, IIndexable, IReadOnlyList<LeasingElementModel>, INotifyCollectionChanged
    {
        /*
            модель для строки с арендами
        */

        readonly List<LeasingElementModel> m_models = new List<LeasingElementModel>();

        private int pv_RowIndex;
        /// <summary>
        /// Возвращает или задаёт Индекс строки в сетке
        /// </summary>
        public int RowIndex { get => pv_RowIndex; set { if (pv_RowIndex != value) { pv_RowIndex = value; OnPropertyChanged(); } } }

        private IReadOnlyList<LeasingElementModel> pv_Leasing;

        void SetCurrentIndex(IEnumerable<LeasingElementModel> leasings)
        {
            if (leasings == null)
                return;

            foreach (var item in leasings)
                ((IIndexable)item).Index = pv_RowIndex;
        }

        /// <summary>
        /// Добавить элемент
        /// </summary>
        /// <param name="model">Новая модель</param>
        public void Add(LeasingElementModel model)
        {
            m_models.Add(model);

            if (CollectionChanged != null)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, model);
                CollectionChanged(this, e);
            }
        }

        string DebugDisplay() => "Count: " + Count.ToString();

        #region INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region IIndexable

        int IIndexable.Index { get => pv_RowIndex; set => pv_RowIndex = value; }

        #endregion

        #region IReadOnlyList<LeasingElementModel>

        public int Count => m_models == null ? 0 : m_models.Count;

        public LeasingElementModel this[int index] => m_models == null ? null : m_models[index];

        public IEnumerator<LeasingElementModel> GetEnumerator()
        {
            return pv_Leasing.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return pv_Leasing.GetEnumerator();
        }

        #endregion

    }
}
