using CarLeasingViewer.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Модель месячной занятости авто
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class MonthBusiness : ViewModelBase
    {
        int setRowIndex = 0;
        int getRowIndex = 0;
        /// <summary>
        /// Актуальные высоты строк на текущей сетке
        /// </summary>
        readonly Dictionary<int, double> m_rowActualHeights = new Dictionary<int, double>();

        readonly IEnumerable<ItemInfo> m_baseCollection;

        internal IEnumerable<ItemInfo> BaseCollection => m_baseCollection;

        internal int BaseCollectionCount { get; private set; }

        internal int CurentCount { get; set; }

        public SearchSettings SearchSettings { get { return App.SearchSettings; } }

        private Month pv_Month;
        /// <summary>
        /// Возвращает или задаёт
        /// </summary>
        public Month Month
        {
            get
            {
                return pv_Month;
            }
            set { if (pv_Month != value) { pv_Month = value; OnPropertyChanged(); } }
        }

        public Month[] Monthes { get; set; }

        private ObservableCollection<ItemInfo> pv_CarBusiness;
        /// <summary>
        /// Возвращает или задаёт
        /// </summary>
        public ObservableCollection<ItemInfo> CarBusiness { get { return pv_CarBusiness; } set { if (pv_CarBusiness != value) { pv_CarBusiness = value; CurentCount = value.Count; OnPropertyChanged(); } } }

        /// <summary>
        /// Возвращает или задаёт Актуальную высоту строки
        /// </summary>
        public double ActualRowHeight
        {
            get { return m_rowActualHeights[getRowIndex++]; }
            set
            {
                m_rowActualHeights.Add(setRowIndex++, value);
                OnPropertyChanged();
            }
        }

        public MonthBusiness() : this(Enumerable.Empty<ItemInfo>()) { }

        public MonthBusiness(IEnumerable<ItemInfo> baseCollection)
        {
            m_baseCollection = baseCollection;
            BaseCollectionCount = baseCollection.Count();

            CarBusiness = new ObservableCollection<ItemInfo>(baseCollection);
        }

        public void ResetFilter()
        {
            CarBusiness = new ObservableCollection<ItemInfo>(m_baseCollection);
        }

        string DebugDisplay()
        {
            return (Month == null ? "NO MONTH" : (Month.Name + " " + Month.Year.ToString())) + " | Count: " + (CarBusiness == null ? "0" : CarBusiness.Count.ToString());
        }
    }
}