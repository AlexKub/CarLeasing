using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarLeasingViewer
{
    public static class SortManager
    {
        //сортировка и поиск по коллекции в одном флаконе)

        /// <summary>
        /// Выборка Занятости по месяцу
        /// </summary>
        /// <param name="sortingCollection">Исходная выборка</param>
        /// <param name="date">Искомая лата</param>
        /// <param name="start">Запустить задачу</param>
        /// <returns>Возвращает НЕ запущенную задачу по выборке</returns>
        [Obsolete("Старая версия сортировки")]
        public static Task<IEnumerable<CarBusiness>> SelectByDay(IEnumerable<CarBusiness> sortingCollection, DateTime date, bool start = true)
        {
            var task = new Task<IEnumerable<CarBusiness>>(() =>
            {
                return sortingCollection
                .Where(cb => cb.Business
                .Any(b => b.Include(date)));
            });

            if (start)
                task.Start();

            return task;
        }

        /// <summary>
        /// Выборка Занятости по пересечению периода
        /// </summary>
        /// <param name="sortingCollection">Исходная выборка</param>
        /// <param name="dateStart">Дата начала периода</param>
        /// <param name="dateEnd">Дата окончания периода</param>
        /// <param name="start">Запустить задачу</param>
        /// <returns>Возвращает НЕ запущенную задачу по выборке</returns>
        [Obsolete("Старая версия сортировки")]
        public static Task<IEnumerable<CarBusiness>> SelectByDays(IEnumerable<CarBusiness> sortingCollection, DateTime dateStart, DateTime dateEnd, bool start = true)
        {
            var task = new Task<IEnumerable<CarBusiness>>(() =>
            {
                return sortingCollection
                .Where(cb => cb.Business
                .Any(b => b.Cross(dateStart, dateEnd)));
            });

            if (start)
                task.Start();

            return task;
        }

        /// <summary>
        /// Выборка Занятости по месяцу
        /// </summary>
        /// <param name="chart">Сортируемый график</param>
        /// <param name="date">Искомая дата</param>
        /// <param name="start">Запустить задачу</param>
        /// <returns>Возвращает задачу по выборке</returns>
        public static Task<IEnumerable<Controls.LeasingChartManagers.RowManager.Row>> SelectByDay(Controls.LeasingChart chart, DateTime date, bool start = true)
        {
            var task = new Task<IEnumerable<Controls.LeasingChartManagers.RowManager.Row>>(() =>
            {
                return chart.RowManager.Rows
                .Where(cb => cb.Bars.Any(b => b?.Model?.Leasing.Include(date) ?? false));
            });

            if (start)
                task.Start();

            return task;
        }

        /// <summary>
        /// Выборка Занятости по пересечению периода
        /// </summary>
        /// <param name="chart">Сортируемый график</param>
        /// <param name="dateStart">Дата начала периода</param>
        /// <param name="dateEnd">Дата окончания периода</param>
        /// <param name="start">Запустить задачу</param>
        /// <returns>Возвращает задачу по выборке</returns>
        public static Task<IEnumerable<Controls.LeasingChartManagers.RowManager.Row>> SelectByDays(Controls.LeasingChart chart, DateTime dateStart, DateTime dateEnd, bool start = true)
        {
            var task = new Task<IEnumerable<Controls.LeasingChartManagers.RowManager.Row>>(() =>
            {
                return chart.RowManager.Rows
                .Where(cb => cb.Bars.Any(b => b?.Model?.Leasing.Cross(dateStart, dateEnd) ?? false));
            });

            if (start)
                task.Start();

            return task;
        }

        /// <summary>
        /// Сортировка моделей по цене аренды за 1 день в порядке возрастания
        /// </summary>
        /// <param name="notSorted">Набор моделей для сортировки</param>
        /// <returns>Возвращает отсортированный или пустой набор</returns>
        public static IEnumerable<CarModel> OrderByPrice(IEnumerable<CarModel> notSorted)
        {
            if (notSorted.IsEmpty())
                return Enumerable.Empty<CarModel>();

            //при тестовых данных нет информации о ценах из БД
            //(т.к., собственно, не факт, что есть подключение к БД)
            if (App.SearchSettings.TestData)
                return Enumerable.Empty<CarModel>();

            //получаем отсортированные стоимости машин
            var priceOrder = DB_Manager.Default.GetDayPriceOrder();

            foreach (var model in notSorted)
            {
                //для каждой машины находим её текущую 'дневную' цену 
                var tuple = priceOrder.FirstOrDefault(t => model.Car != null && t.Item1.Equals(model.Car.No));

                if (tuple != null)
                {
                    var price = model.Price;
                    //обновляем ценники для сортировки
                    model.Price = new CarPriceList(tuple.Item2, price.Week, price.Long);
                }
            }

            //сортируем по минимальной цене
            return notSorted.OrderBy(m => m.Price.Day).ToList();
        }
    }
}
