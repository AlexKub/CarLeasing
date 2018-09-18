using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CarLeasingViewer.Models.Selections
{
    public abstract class TabItemSelection<TKey> : ViewModels.ViewModelBase, IEnumerable<MonthBusiness>
    {
        public TKey SelectionKey { get; private set; }

        private ObservableCollection<MonthBusiness> _Results;
        /// <summary>
        /// Возвращает или задаёт 
        /// </summary>
        public ObservableCollection<MonthBusiness> Results { get { return _Results; } set { _Results = value; OnPropertyChanged(); } }

        public TabItemSelection(TKey key)
        {
            SelectionKey = key;
        }

        public TabItemSelection(TKey key, IEnumerable<MonthBusiness> results) : this(key)
        {

            Results = new ObservableCollection<MonthBusiness>(results);
        }

        public IEnumerator<MonthBusiness> GetEnumerator()
        {
            if (Results == null)
                return Enumerable.Empty<MonthBusiness>().GetEnumerator();

            return  Results.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (Results == null)
                return Enumerable.Empty<MonthBusiness>().GetEnumerator();

            return Results.GetEnumerator();
        }

        public void Add(MonthBusiness mb)
        {
            if (Results == null)
                Results = new ObservableCollection<MonthBusiness>();

            Results.Add(mb);
        }
    }
}
