﻿namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Арендные платы на авто
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class CarPriceList
    {
        /// <summary>
        /// Единственный пустой экземпляр
        /// </summary>
        public static readonly CarPriceList Default = new CarPriceList();

        /// <summary>
        /// Получение цены по количеству дней
        /// </summary>
        /// <param name="dayCount">Количество дней</param>
        /// <returns>Возвращает соответствующую количеству дней цену</returns>
        public decimal this[int dayCount]
        {
            get
            {
                switch (dayCount)
                {
                    case 0:
                        return 0m;
                    case 1:
                    case 2:
                        return Day;
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        return Week;
                    default:
                        return Long;
                }
            }
        }

        /// <summary>
        /// От 1 до 2 дней
        /// </summary>
        public decimal Day { get; private set; }

        /// <summary>
        /// От 3 до 6 дней
        /// </summary>
        public decimal Week { get; private set; }

        /// <summary>
        /// От 7 дней и больше
        /// </summary>
        public decimal Long { get; private set; }

        /// <summary>
        /// Проверка на наличие значений
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (this == Default)
                    return true;

                const decimal zero = 0m;

                return Day == zero
                    && Week == zero
                    && Long == zero;
            }
        }

        #region constructors

        private CarPriceList() { } //нечего пустые экземпляры плодить

        /// <summary>
        /// Цена на авто
        /// </summary>
        /// <param name="day">От 1 до 2 дней</param>
        public CarPriceList(decimal day)
        {
            Day = day;
        }

        /// <summary>
        /// Цены на авто
        /// </summary>
        /// <param name="day">От 1 до 2 дней</param>
        /// <param name="week">От 3 до 6 дней</param>
        public CarPriceList(decimal day, decimal week)
        {
            Day = day;
            Week = week;
        }

        /// <summary>
        /// Цены на авто
        /// </summary>
        /// <param name="day">От 1 до 2 дней</param>
        /// <param name="week">От 3 до 6 дней</param>
        /// <param name="longer">Более 7 дней</param>
        public CarPriceList(decimal day, decimal week, decimal longer)
        {
            Day = day;
            Week = week;
            Long = longer;
        }


        #endregion

        /// <summary>
        /// Получение цены по количеству дней
        /// </summary>
        /// <param name="dayCount">Количество дней</param>
        /// <returns>Возвращает соответствующуюю количеству дней цену</returns>
        public decimal GetPriceForPeriod(int dayCount) => this[dayCount];

        string DebugDisplay()
        {
            const string format = "F2";

            return $"{Day.ToString(format)} | {Week.ToString(format)} | {Long.ToString(format)}";
        }

    }
}
