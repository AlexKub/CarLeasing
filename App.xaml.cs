﻿using CarLeasingViewer.Models;
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

        public static LeasingSet CurrentSet { get; set; }

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

        public static IEnumerable<int> AvailableYears { get; private set; } = Enumerable.Empty<int>();

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

        public static MainWindow2 GetMainWindow()
        {
            return App.Current.MainWindow as MainWindow2;
        }

        /// <summary>
        /// Простановка списка доступных методов из БД
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
    }
}
