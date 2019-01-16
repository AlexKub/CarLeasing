using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarLeasingViewer
{
    public static class SortManager
    {
        //сортировка и поиск по коллекции

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
            if (App.TestMode && App.SearchSettings.TestData)
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
            //и имени, на случае отсутствия цены
            var result = from m in notSorted
                         orderby m.Price.Day, m.Text
                         select m;

            return result;
        }

        /// <summary>
        /// Выбор свободных машин в указанные даты
        /// </summary>
        /// <param name="collection">Список машин</param>
        /// <param name="dateStart">Дата начала (включительно)</param>
        /// <param name="dateEnd">Дата окончания (включительно)</param>
        /// <returns>Возвращает новый список, свободных в указанные даты</returns>
        public static IList<Controls.LeasingChartManagers.RowManager.Row> SelectFree(this IEnumerable<Controls.LeasingChartManagers.RowManager.Row> collection, DateTime dateStart, DateTime dateEnd)
        {
            if (collection.IsEmpty())
            {
                return collection?.ToList() ?? Enumerable.Empty<Controls.LeasingChartManagers.RowManager.Row>().ToList();
            }
            else
            { 
                var sorted = new List<Controls.LeasingChartManagers.RowManager.Row>();
                bool busy = false;

                foreach (var row in collection)
                {
                    if (row.Bars.Count > 0) //если авто арендовано
                    {
                        //для каждой аренды
                        foreach (var bar in row.Bars) 
                        {
                            if (bar.Model == null || bar.Model.Leasing == null)
                                continue;

                            var l = bar.Model.Leasing;
                            //проверка, что аренда пересекается с указанным периодом
                            if (l.Cross(dateStart, dateEnd)
                                //проверка, что аренда не заканчивается на начале выбранного периода
                                //в таком случае, машина может освободиться во второй половине интересующего срока
                                && l.DateEnd.Date != dateStart.Date)
                            {
                                //если машина занята - идём дальше
                                busy = true;
                                break;
                            }
                        }

                        if (!busy)
                            //выбираем те машины, что свободны
                            sorted.Add(row);

                        busy = false;
                    }
                    else
                        sorted.Add(row);
                }

                return sorted;
            }
        }

        
    }
}
