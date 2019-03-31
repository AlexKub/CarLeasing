using System;

namespace CarLeasingViewer
{
    /// <summary>
    /// Логика управления Периодами
    /// </summary>
    public static class PeriodManaging
    {
        /// <summary>
        /// Пересчёт количества месяцев в Периоде
        /// </summary>
        /// <param name="period">Период</param>
        /// <returns>Возвращает количество месяцев в Периоде</returns>
        public static int CalculateMonthCount(this IPeriod period)
        {
            if (period == null)
                return 0;

            var dateStart = period.DateStart;
            var dateEnd = period.DateEnd;

            //если есть разница в годе (взята в одном году,а возвращается в другом)
            if (dateStart.Year != dateEnd.Year)
            {
                var monthCount = dateEnd.Month; //количество месяцев в крайнем году

                int yearCount = dateEnd.Year - dateStart.Year;
                int curentYear = dateStart.Year;
                for (int y = 0; y < yearCount; y++)
                {
                    if (curentYear == dateStart.Year) //если сейчас начальный год
                        monthCount += ((12 - dateStart.Month) + 1); //добавляем количество месяцев в текущем году, за вычетом прошедших
                    else
                        monthCount += 12; //добавляем все 12 месяцев, если мышина взята на период более 2 лет О_о

                    curentYear++;
                }

                return monthCount;
            }
            else
                return (dateEnd.Month - dateStart.Month) + 1;
        }

        /// <summary>
        /// Проверка наличия даты в периоде
        /// </summary>
        /// <param name="period">Период проверки</param>
        /// <param name="date">Дата</param>
        /// <returns>Возвращает наличие даты в периоде</returns>
        public static bool Include(this IPeriod period, DateTime date)
        {
            return (period.DateStart <= date) && (date <= period.DateEnd);
        }

        /// <summary>
        /// Проверка пересечения периодов
        /// </summary>
        /// <param name="period">Первый период</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static bool Cross(this IPeriod this_period, IPeriod period)
        {
            return !((this_period.DateStart > period.DateEnd) || (this_period.DateEnd < period.DateStart));
        }
    }
}
