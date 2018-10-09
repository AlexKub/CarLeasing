using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarLeasingViewer
{
    /// <summary>
    /// Общее управление источниками загрузки данных в приложении
    /// </summary>
    public static class DataManager
    {
        /// <summary>
        /// Получает данные по занятости авто между указанными месяцами (включительно)
        /// </summary>
        /// <param name="first">Месяц начала</param>
        /// <param name="last">Месяц окончания</param>
        /// <returns></returns>
        public static MonthBusiness[] GetDataset(Month first, Month last)
        {
            MonthBusiness[] monthBuisnesses = null;

            if (App.SearchSettings.TestData)
            {
                var rMonth = Randomizer.GetRandomMonth(2018);
                List<MonthBusiness> businesses = new List<MonthBusiness>();

                foreach (var month in Month.GetMonthes(first, last))
                {
                    businesses.Add(Randomizer.GetRandomBusiness(month));
                }

                return businesses.ToArray();
            }
            else
            {
                monthBuisnesses = new MonthBusiness[] { DB_Manager.Default.GetBusinessByMonthes(first, last) };
            }

            return monthBuisnesses;
        }

        [Obsolete]
        public static void FillMainWindow2ViewModel(ViewModels.LeasingViewViewModel viewModel)
        {
            if (viewModel == null)
                throw new NullReferenceException("Передана пустая модель окна для заполнения");

            var startMonth = Randomizer.GetRandomMonth(2017);
            var rMonth = Randomizer.GetRandomMonth(2018);
            var buisnesses = App.SearchSettings.TestData 
                ? new MonthBusiness[] { Randomizer.GetRandomBusiness(rMonth), Randomizer.GetRandomBusiness(rMonth.Next()), Randomizer.GetRandomBusiness(rMonth.Next(2)) }
                : new MonthBusiness[] { DB_Manager.Default.GetBusinessByMonth(startMonth), DB_Manager.Default.GetBusinessByMonth(startMonth.Next()), DB_Manager.Default.GetBusinessByMonth(startMonth.Next().Next()) };
            viewModel.Cars = buisnesses
                .SelectMany(mb => mb.CarBusiness)
            .Select(cb => cb.Name)
            .Distinct()
            .Select(name => new CarModel() { Text = name }).ToList();

            viewModel.Comments = viewModel.Cars.Select(car => new CarComment() { RowIndex = car.RowIndex, Comment = (car.Text + "_comment") }).ToList();
            //tabItem.MonthLeasing.CarBusiness.Select(b => new CarHeaderModel() { Text = b.Name }).ToList();

            var leasingBarModels = new List<LeasingElementModel>[buisnesses.Length];

            var index = 0;
            //foreach (var item in tabItem.MonthLeasing.CarBusiness)
            foreach (var business in buisnesses)
            {
                foreach (var item in business.CarBusiness)
                {

                    var car = viewModel.Cars.FirstOrDefault(c => c.Text.Equals(item.Name));
                    if (leasingBarModels[index] == null)
                        leasingBarModels[index] = new List<LeasingElementModel>();

                    leasingBarModels[index].AddRange(item.Business.Select(b => new LeasingElementModel() { Leasing = b, RowIndex = car == null ? 0 : car.RowIndex }));
                }
                index++;
            }

            index = 0;
            viewModel.MonthLeasings = new List<MonthLeasing>(
                buisnesses.Select(bus =>
                new MonthLeasing()
                {
                    ColumnIndex = index,
                    //MonthHeader = new MonthHeaderModel() { Month = bus.Month, ColumnIndex = index },
                    Leasings = leasingBarModels[index++],
                }));
        }
    }
}
