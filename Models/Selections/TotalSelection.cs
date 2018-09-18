using System.Collections.Generic;

namespace CarLeasingViewer.Models.Selections
{
    public class TotalSelection : TabItemSelection<string>
    {
        public string Title { get { return SelectionKey; } }

        
        public TotalSelection(string key, IEnumerable<MonthBusiness> results) : base(key, results) { }

        public TotalSelection(string key) : base(key)
        {
        }
    }
}
