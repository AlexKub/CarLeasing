namespace System
{
    public static class DateTimeExtensions
    {
        public static DateTime GetEndOfMonth(this DateTime date)
        {
            var nextDate = date.Date.AddDays(1);

            while (nextDate.Month == date.Month)
            {
                date = nextDate;

                nextDate = date.Date.AddDays(1);
            }

            return date.Date;
        }
    }
}
