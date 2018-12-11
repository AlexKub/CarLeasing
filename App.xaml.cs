using CarLeasingViewer.Models;
using CarLeasingViewer.Views;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CarLeasingViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static LogSet m_loger = LogManager.GetDefaultLogSet<App>();
        public static LogSet Loger { get { return m_loger; } }

        /// <summary>
        /// Тестовый режим
        /// </summary>
        public static bool TestMode { get; private set; }

        public static IDictionary<int, IEnumerable<Month>> AvailableMonthes { get; private set; } = new Dictionary<int, IEnumerable<Month>>();

        public static IEnumerable<Month> AvailableMonthesAll
        {
            get
            {
                var list = new List<Month>();

                foreach (var item in AvailableMonthes)
                    list.AddRange(item.Value);

                return list;
            }
        }

        public static IEnumerable<Car> Cars { get; private set; } = Enumerable.Empty<Car>();

        public static IEnumerable<int> AvailableYears { get; private set; } = Enumerable.Empty<int>();

        public static IEnumerable<Region> Regions { get; private set; } = Enumerable.Empty<Region>();

        public App()
        {
            //AvailableYears = new int[] { 2017, 2018 };

            //var monthes = new Dictionary<int, IEnumerable<Month>>();

            //foreach (var year in AvailableYears)
            //    monthes.Add(year, Month.GetMonthes());

            //AvailableMonthes = monthes;
        }

        public static ApplicationSearchSettings SearchSettings { get { return ApplicationSearchSettings.Instance; } }

        static App()
        {
            try
            {
                LogManager.Settings.LogSetFlag = LogManager.LogSetFlag_File;
            }
            finally { }

            //http://www.abhisheksur.com/2011/03/deal-with-cpu-usage-in-wpf-applications.html
            //Timeline.DesiredFrameRateProperty.OverrideMetadata(
            //    typeof(Timeline),
            //    new FrameworkPropertyMetadata { DefaultValue = 25 } );
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var args = e.Args;

            if(args != null && args.Length > 0)
            {
                //тестовый режим
                TestMode = args.Any(a => a.Equals("test"));
            }

            base.OnStartup(e);
        }

        public static MainWindow GetMainWindow()
        {
            return App.Current.MainWindow as MainWindow;
        }

        /// <summary>
        /// Простановка списка доступных месяцев из БД
        /// </summary>
        /// <param name="monthes">Месяцы из БД</param>
        public static void SetAvailable(IEnumerable<Month> monthes)
        {
            if(monthes == null)
            {
                AvailableYears = Enumerable.Empty<int>();
                AvailableMonthes = new Dictionary<int, IEnumerable<Month>>();
                return;
            }

            var groupedMonthes = monthes.GroupBy(m => m.Year);
            AvailableYears = groupedMonthes.Select(g => g.Key);
            AvailableMonthes = groupedMonthes.Select(g => g.AsEnumerable()).ToDictionary((v) => v.First().Year);
        }

        /// <summary>
        /// Простановка общего набора машин
        /// </summary>
        /// <param name="cars">Набор машин из БД</param>
        public static void SetCars(IEnumerable<Car> cars)
        {
            if (cars == null || cars.Count() == 0)
                return;

            Cars = cars.ToList();
        }

        /// <summary>
        /// Простановка общего набора регионов
        /// </summary>
        /// <param name="regions">Набор регионов из БД</param>
        public static void SetRegions(IEnumerable<Region> regions)
        {
            if (regions == null || regions.Count() == 0)
                return;

            Regions = regions.ToList();
        }
    }
}
