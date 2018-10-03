using CarLeasingViewer.Log;
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

        /// <summary>
        /// Начало ID'шника элемента (с подчёркиванием)
        /// </summary>
        string m_prefixID;

        /// <summary>
        /// Отсортированный список занятости по дате начала
        /// </summary>
        readonly List<LeasingElementModel> m_models = new List<LeasingElementModel>();

        private int pv_RowIndex;
        /// <summary>
        /// Возвращает или задаёт Индекс строки в сетке
        /// </summary>
        public int RowIndex
        {
            get => pv_RowIndex; set
            {
                if (pv_RowIndex != value)
                {
                    pv_RowIndex = value;

                    m_prefixID = value.ToString() + "_"; //кешируем первую часть, чтобы постоянно не делать впоследствии

                    SetCurrentRowIndex();

                    OnPropertyChanged();
                }
            }
        }

        private IReadOnlyList<LeasingElementModel> pv_Leasing;

        /// <summary>
        /// Проставление индекса строки
        /// </summary>
        void SetCurrentRowIndex()
        {
            foreach (var item in m_models)
                ((IIndexable)item).Index = pv_RowIndex;
        }

        /// <summary>
        /// Добавить элемент
        /// </summary>
        /// <param name="model">Новая модель</param>
        public void Add(LeasingElementModel model)
        {
            if (model == null)
            {
                App.Loger.Log("Передана пустая ссылка на модель в 'LeasingElementsCollectionModel'", MessageType.Error);
                return;
            }

            if (model.Leasing == null)
            {
                App.Loger.Log("Передана модель Аренды без данных Аренды. Модель пропущена", MessageType.Error
                    , new SourceParameter(this));
                return;
            }

            bool added = false;
            //сортируем для простановки индекса
            int i = 0;
            for (i = 0; i < m_models.Count; i++)
            {
                if (m_models[i].Leasing.DateStart > model.Leasing.DateStart)
                {
                    m_models.Insert(i, model);
                    added = true;
                    model.ElementID = m_prefixID + i.ToString();
                    break;
                }
            }

            //если у остальных дата начала раньше или коллекция пуста
            if (!added)
            {
                i = m_models.Count;
                m_models.Add(model);
                model.ElementID = m_prefixID + i.ToString();
            }

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
