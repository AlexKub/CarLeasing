using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarLeasingViewer
{
    /// <summary>
    /// Общее управление источниками загрузки данных в приложении
    /// </summary>
    public static class DataManager
    {
        public static void FillMainWindow2ViewModel(ViewModels.LeasingViewViewModel viewModel)
        {
            if (viewModel == null)
                throw new NullReferenceException("Передана пустая модель окна для заполнения");

            var startMonth = Randomizer.GetRandomMonth(2017);
            var buisnesses = App.SearchSettings.TestData 
                ? new MonthBusiness[] { Randomizer.GetRandomBusiness(), Randomizer.GetRandomBusiness(), Randomizer.GetRandomBusiness() }
                : new MonthBusiness[] { DB_Manager.Default.GetBusinessByMonth(startMonth), DB_Manager.Default.GetBusinessByMonth(startMonth.Next()), DB_Manager.Default.GetBusinessByMonth(startMonth.Next().Next()) };
            viewModel.Cars = buisnesses
                .SelectMany(mb => mb.CarBusiness)
            .Select(cb => cb.Name)
            .Distinct()
            .Select(name => new CarHeaderModel() { Text = name }).ToList();

            viewModel.Comments = viewModel.Cars.Select(car => new CarComment() { RowIndex = car.RowIndex, Comment = (car.Text + "_comment") }).ToList();
            //tabItem.MonthLeasing.CarBusiness.Select(b => new CarHeaderModel() { Text = b.Name }).ToList();

            var leasingBarModels = new List<LeasingBarModel>[buisnesses.Length];

            var index = 0;
            //foreach (var item in tabItem.MonthLeasing.CarBusiness)
            foreach (var business in buisnesses)
            {
                foreach (var item in business.CarBusiness)
                {

                    var car = viewModel.Cars.FirstOrDefault(c => c.Text.Equals(item.Name));
                    if (leasingBarModels[index] == null)
                        leasingBarModels[index] = new List<LeasingBarModel>();

                    leasingBarModels[index].AddRange(item.Business.Select(b => new LeasingBarModel() { Leasing = b, RowIndex = car == null ? 0 : car.RowIndex }));
                }
                index++;
            }

            index = 0;
            viewModel.MonthLeasings = new List<MonthLeasing>(
                buisnesses.Select(bus =>
                new MonthLeasing()
                {
                    ColumnIndex = index,
                    MonthHeader = new MonthHeaderModel() { Month = bus.Month, ColumnIndex = index },
                    Leasings = leasingBarModels[index++],
                }));
        }
    }
}
