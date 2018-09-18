using System.Collections.Generic;

namespace CarLeasingViewer.Models.Selections
{
    /// <summary>
    /// Выборка по году
    /// </summary>
    public class YearSelection : TabItemSelection<int>
    {
        public string YearStr { get { return SelectionKey.ToString(); } }

        public int Year { get { return SelectionKey; } }

        public YearSelection(int key, IEnumerable<MonthBusiness> results) : base(key, results) { }

        public YearSelection(int key) : base(key)
        {
        }
    }
}
