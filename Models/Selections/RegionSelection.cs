using System.Collections.Generic;

namespace CarLeasingViewer.Models.Selections
{
    public class RegionSelection : TabItemSelection<Region>
    {
        public Region Region { get { return SelectionKey; } }

        public string DisplayName { get { return SelectionKey == null ? "NO REGION" : SelectionKey.DisplayName; } }

        public RegionSelection(Region key, IEnumerable<MonthBusiness> results) : base(key, results)
        {
        }

        public RegionSelection(Region key) : base(key)
        {
        }
    }
}
