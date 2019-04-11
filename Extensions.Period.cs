using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarLeasingViewer
{
	/// <summary>
    /// Расширения для Периода
    /// </summary>
    public static class PeriodExtensions
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
            return this_period.DayIndexStart < period.DayIndexStart
                        ? period.DayIndexStart <= this_period.DayIndexEnd
                        : this_period.DayIndexStart <= period.DayIndexEnd; 
        }

        /// <summary>
        /// Количество пересекаемых дней
        /// </summary>
        /// <param name="period">Основной период</param>
        /// <param name="start">Сравниваемый период</param>
        /// <returns></returns>
        public static int CrossDaysCount(this IPeriod this_period, IPeriod period)
        {
            int crossStart = 0;
            int crossEnd = 0;

            if (this_period.DayIndexStart < period.DayIndexStart)
            {
                crossStart = period.DayIndexStart;
                crossEnd = this_period.DayIndexEnd > period.DayIndexEnd 
                    ? period.DayIndexEnd
                    : this_period.DayIndexEnd;
            }
            else
            {
                crossStart = this_period.DayIndexStart;
                crossEnd = period.DayIndexEnd > this_period.DayIndexEnd
                    ? this_period.DayIndexEnd
                    : period.DayIndexEnd;
            }

            var count = crossEnd - crossStart + 1;
            if (count < 0)
                return 0;

            return count;
        }


        public static string TooltipRow(this IPeriod period)
        {
            var sb = new StringBuilder();

            return AppendTooltipRow(period, sb).ToString();
        }
        /// <summary>
        /// Строковое представление
        /// </summary>
        /// <param name="period">Период</param>
        /// <param name="inline">Отступ</param>
        /// <param name="sb">Текущий экземпляр StringBuilder</param>
        /// <returns></returns>
        public static StringBuilder AppendTooltipRow(this IPeriod period, StringBuilder sb, string inline = "    ")
        {
            //копипаста из BussinessDateConverter (старая версия)
            if (sb == null)
                sb = new StringBuilder();

            //<действие> c XX по ХХ <месяц>
            sb.Append(inline).Append("c ");

            sb.Append(period.DateStart.Day.ToString()).Append(" ")
                .Append(period.DateStart.GetMonthName()).Append(" ")
                .Append(period.DateStart.Year.ToString()).Append(" ")
                .AppendLine(period.DateStart.TimeOfDay.Hours > 0 ? period.DateStart.TimeOfDay.ToString(@"hh\:mm") : string.Empty)
                .Append(inline)
                .Append("по ")
                .Append(period.DateEnd.Day.ToString()).Append(" ")
                .Append(period.DateEnd.GetMonthName()).Append(" ")
                .Append(period.DateEnd.Year.ToString()).Append(" ")
                .Append(period.DateEnd.TimeOfDay.Hours > 0 ? period.DateEnd.TimeOfDay.ToString(@"hh\:mm") : string.Empty);

            return sb;
        }

        /// <summary>
        /// Подсчёт количества дней в Периоде
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        public static int DaysCount(this IPeriod period)
        {
            if (period == null)
                return 0;

            return (period.DateEnd.Date - period.DateStart.Date).Days + 1;
        }
    }
}
