using CarLeasingViewer.Models;
using System.Windows;
using System.Windows.Controls;

namespace CarLeasingViewer.Converters
{
    public class MainTabItemSelector : DataTemplateSelector
    {
        public DataTemplate OneMonthTemplate { get; set; }

        public DataTemplate PeriodTemplate { get; set; }

        public DataTemplate YearTemplate { get; set; }

        public DataTemplate RegionTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null || !(item is TabItemModel))
                return null;

            if (item is PeriodTabItemModel)
                return PeriodTemplate;
            if (item is OneMonthItem)
                return OneMonthTemplate;
            if (item is RegionTabItemModel)
                return RegionTemplate;

            return null;
        }
    }
}
