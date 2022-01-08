namespace System
{
    public static class DayOfWeekExtensions
    {
        /// <summary>
        /// Следующий день недели
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public static DayOfWeek Next(this DayOfWeek current)
        {
            switch (current)
            {
                case DayOfWeek.Sunday:
                    return DayOfWeek.Monday;
                case DayOfWeek.Monday:
                    return DayOfWeek.Tuesday;
                case DayOfWeek.Tuesday:
                    return DayOfWeek.Wednesday;
                case DayOfWeek.Wednesday:
                    return DayOfWeek.Thursday;
                case DayOfWeek.Thursday:
                    return DayOfWeek.Friday;
                case DayOfWeek.Friday:
                    return DayOfWeek.Saturday;
                case DayOfWeek.Saturday:
                    return DayOfWeek.Sunday;
                default:
                    return current;
            }
        }

        /// <summary>
        /// Сокращенное наименование месяца
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetShortName(this DayOfWeek type)
        {
            switch (type)
            {
                case DayOfWeek.Monday:
                    return "пн";
                case DayOfWeek.Tuesday:
                    return "вт";
                case DayOfWeek.Wednesday:
                    return "ср";
                case DayOfWeek.Thursday:
                    return "чт";
                case DayOfWeek.Friday:
                    return "пт";
                case DayOfWeek.Saturday:
                    return "сб";
                case DayOfWeek.Sunday:
                    return "вс";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Полное имя месяца
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetName(this DayOfWeek type)
        {
            switch (type)
            {
                case DayOfWeek.Monday:
                    return "понедельник";
                case DayOfWeek.Tuesday:
                    return "вторник";
                case DayOfWeek.Wednesday:
                    return "среда";
                case DayOfWeek.Thursday:
                    return "четверг";
                case DayOfWeek.Friday:
                    return "пятница";
                case DayOfWeek.Saturday:
                    return "суббота";
                case DayOfWeek.Sunday:
                    return "воскресенье";
                default:
                    return string.Empty;
            }
        }
    }
}
