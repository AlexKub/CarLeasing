using CarLeasingViewer.Models;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using CarLeasingViewer.ViewModels;
using System;

namespace CarLeasingViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Random m_rand = new System.Random();

        Month m_month = new Month(2018, Monthes.September);

        int windowLoaded = 0;
        int exceptionCount = 0;

        public MainWindow()
        {
            UpdateSearchSettings();

            InitializeComponent();

            windowLoaded++;
        }

        public void UpdateSearchSettings()
        {
            var vm = DataContext as MainWindowViewModel;

            bool dataContextEmpty = vm == null;
            if (dataContextEmpty)
                vm = new MainWindowViewModel();

            if (App.SearchSettings.TestData)
                SetTestData(vm);
            else
                SetFromDB(vm);

            if (dataContextEmpty)
                DataContext = vm;
        }

        void SetTestData(MainWindowViewModel vm)
        {
            vm.CurrentMonthBusiness = GetRandomBusiness();

            vm.TotalBusiness = new Models.Selections.TotalSelection("Общее",
                new MonthBusiness[] { Randomizer.GetRandomBusiness(), Randomizer.GetRandomBusiness(), Randomizer.GetRandomBusiness() });

            var monthes = vm.TotalBusiness.Select(b => b.Month).Distinct();

            vm.TabItemsModels = GetTabItemsModels(monthes, vm.TotalBusiness);
        }

        void SetFromDB(MainWindowViewModel vm)
        {
            var db = DB_Manager.Default;

            //результаты для вкладки текущего месяца
            vm.CurrentMonthBusiness = db.GetBusinessByMonth(Month.Current);

            var curentMonth = vm.CurrentMonthBusiness.Month;
            vm.TotalBusiness = new Models.Selections.TotalSelection("Общее");

            //получение всех доступных месяцев
            var availableMonthes = db.GetAvailableMonthes();
            App.SetAvailable(availableMonthes);

            foreach (var month in App.AvailableMonthesAll)
                vm.TotalBusiness.Add(db.GetBusinessByMonth(month));

            var monthes = vm.TotalBusiness.Select(b => b.Month).Distinct();

            vm.TabItemsModels = GetTabItemsModels(monthes, vm.TotalBusiness);
        }

        TabItemModel[] GetTabItemsModels(IEnumerable<Month> monthes, IEnumerable<MonthBusiness> leasings)
        {
            var leasing = leasings.FirstOrDefault();
            var monthItem = new OneMonthItem(leasing == null ? "NULL" : leasing.Month.Name)
            {
                MonthLeasing = leasing
            };
            var periodItem = new PeriodTabItemModel("За период")
            {
                AvailableMonthes = monthes,
                AvailableYears = monthes.Select(m => m.Year).Distinct(),
                FromMonth = monthes.FirstOrDefault(),
                ToMonth = monthes.LastOrDefault(),
            };
            periodItem.AddRange(leasings);

            var region = new Region() { DisplayName = "Казань", PostCode = "134064", Address = "г. Казань, ул. Кирова, д. 1а" };
            var kazan = new RegionTabItemModel(region.DisplayName)
            {
                Region = region,
                AvailableMonthes = monthes,
                AvailableYears = monthes.Select(m => m.Year).Distinct(),
                FromMonth = monthes.FirstOrDefault(),
                ToMonth = monthes.LastOrDefault(),
            };
            kazan.AddRange(leasings);

            return new TabItemModel[] { monthItem, periodItem, kazan };
        }

        MonthBusiness GetRandomBusiness()
        {
            var mb = new MonthBusiness(Generate());
            mb.Month = m_month;
            //            System.Windows.Media.Brushes.LightGray
            return mb;
        }

        MonthBusiness GetDBBusiness(Month m = null)
        {
            var db = DB_Manager.Default;

            var available = Enumerable.Empty<Month>();
            if (m == null)
                available = db.GetAvailableMonthes(App.SearchSettings);

            var mb = DB_Manager.Default.GetBusinessByMonth(m == null ? available.First() : m, App.SearchSettings);

            return mb;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var mvm = DataContext as ViewModels.MainWindowViewModel;

            mvm.MonthD = new Month(2018, Monthes.August);
        }

        IEnumerable<CarBusiness> Generate()
        {
            List<CarBusiness> values = new List<CarBusiness>();
            for (int i = 0; i < 100; i++)
            {
                values.Add(GenerateBusiness(i));
            }

            return values;
        }

        CarBusiness GenerateBusiness(int index)
        {
            var cb = new CarBusiness();
            cb.Name = "Car_" + index.ToString();
            cb.Month = m_month;

            var count = m_rand.Next(1, 10);

            int start = 1;
            int end = 1;

            var dayCount = m_month.DayCount + 1;

            for (int i = 0; i < count && end < dayCount; i++)
            {
                var b = new Business();
                b.Title = "bussy_" + i.ToString();
                start = m_rand.Next(end, dayCount);
                b.DateStart = new DateTime(m_month.Year, m_month.Index, start);
                end = m_rand.Next(start, dayCount);
                b.DateEnd = new DateTime(m_month.Year, m_month.Index, end);
                b.CurrentMonth = m_month;
                cb.Add(b);
                end++;
            }

            return cb;
        }

        private void CachedTabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var curentVM = DataContext as MainWindowViewModel;
            var tabItem = (curentVM.TabItemsModels.First() as OneMonthItem);

            var vm = new LeasingViewViewModel();
            MonthBusiness[] monthBuisnesses = null;

            if (App.SearchSettings.TestData)
            {
                monthBuisnesses = new MonthBusiness[] { Randomizer.GetRandomBusiness(), Randomizer.GetRandomBusiness(), Randomizer.GetRandomBusiness() };
            }
            else
            {
                var second = curentVM.TabItemsModels.ElementAt(1) as PeriodTabItemModel;
                monthBuisnesses = new MonthBusiness[] { DB_Manager.Default.GetBusinessByMonthes(App.AvailableMonthesAll.First(), App.AvailableMonthesAll.Last()) };
            }

            var set = new LeasingSet();
            set.Data = monthBuisnesses;
            vm.LeasingSet = set;
            vm.Cars = monthBuisnesses
                .SelectMany(mb => mb.CarBusiness)
            .Select(cb => cb.Name)
            .Distinct()
            .Select(name => new CarModel() { Text = name }).ToList();

            vm.Comments = vm.Cars.Select(car => new CarComment() { RowIndex = car.RowIndex, Comment = (car.Text + "_comment") }).ToList();
            //tabItem.MonthLeasing.CarBusiness.Select(b => new CarHeaderModel() { Text = b.Name }).ToList();

            var leasingBarModels = new List<LeasingElementModel>[monthBuisnesses.Length];

            var index = 0;
            //foreach (var item in tabItem.MonthLeasing.CarBusiness)
            foreach (var business in monthBuisnesses)
            {
                foreach (var item in business.CarBusiness)
                {

                    var car = vm.Cars.FirstOrDefault(c => c.Text.Equals(item.Name));
                    if (leasingBarModels[index] == null)
                        leasingBarModels[index] = new List<LeasingElementModel>();

                    leasingBarModels[index].AddRange(item.Business.Select(b => new LeasingElementModel() { Leasing = b, RowIndex = car == null ? 0 : car.RowIndex, DayColumnWidth = 21d }));
                }
                index++;
            }

            index = 0;
            vm.MonthLeasings = new List<MonthLeasing>(
                monthBuisnesses.Select(bus =>
                new MonthLeasing()
                {
                    ColumnIndex = index,
                    MonthHeader = new MonthHeaderModel() { Month = bus.Month },
                    Leasings = leasingBarModels[index++]
                }));

            new Views.MainWindow2(vm).ShowDialog();
        }
    }
}
