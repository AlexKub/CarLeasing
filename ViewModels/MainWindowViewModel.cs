using System.Collections.Generic;
using CarLeasingViewer.Models;
using CarLeasingViewer.Models.Selections;

namespace CarLeasingViewer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Month _Month;
        /// <summary>
        /// Возвращает или задаёт Месяц
        /// </summary>
        public Month MonthD { get { return _Month; } set { _Month = value; OnPropertyChanged(); } }

        private MonthBusiness _CurrentMonthBusiness = new MonthBusiness();
        /// <summary>
        /// Возвращает или задаёт Месячную занятость
        /// </summary>
        public MonthBusiness CurrentMonthBusiness { get { return _CurrentMonthBusiness; } set { _CurrentMonthBusiness = value; OnPropertyChanged(); } }

        private TotalSelection _TotalBusiness;
        /// <summary>
        /// Возвращает или задаёт 
        /// </summary>
        public TotalSelection TotalBusiness { get { return _TotalBusiness; } set { _TotalBusiness = value; OnPropertyChanged(); } }

        private IEnumerable<Region> _Regions;
        /// <summary>
        /// Возвращает или задаёт Набор регионов
        /// </summary>
        public IEnumerable<Region> Regions { get { return _Regions; } set { _Regions = value; OnPropertyChanged(); } }

        private IEnumerable<TabItemModel> pv_TabItemsModels;
        /// <summary>
        /// Возвращает или задаёт
        /// </summary>
        public IEnumerable<TabItemModel> TabItemsModels { get { return pv_TabItemsModels; } set { if (pv_TabItemsModels != value) { pv_TabItemsModels = value; OnPropertyChanged(); } } }

        public SearchSettings SearchSettings { get { return App.SearchSettings; } }

        public MainWindowViewModel()
        {
            MonthD = new Month(2018, Monthes.July);
        }
    }
}
