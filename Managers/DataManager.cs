using CarLeasingViewer.Models;
using System.Collections.Generic;

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

            if (App.TestMode && App.SearchSettings.TestData)
            {
                var rMonth = Randomizer.GetRandomMonth(2018);
                List<MonthBusiness> businesses = new List<MonthBusiness>();
                
                foreach (var month in Month.GetMonthes(first, last))
                {
                    businesses.Add(Randomizer.GetRandomBusiness(month));
                }

                //return new MonthBusiness[] { Randomizer.GetRandomBusiness(first, last) };

                return businesses.ToArray();
            }
            else
            {
                var db = DB_Manager.Default;
                App.SetAvailable(DB_Manager.Default.GetAvailableMonthes());
                App.SetCars(DB_Manager.Default.GetAllCars(first, last));
                App.SetRegions(DB_Manager.Default.GetRegions());

                monthBuisnesses = new MonthBusiness[] { db.GetBusinessByMonthes(first, last) };
            }

            return monthBuisnesses;
        }

    }
}
