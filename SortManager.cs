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


    }
}
