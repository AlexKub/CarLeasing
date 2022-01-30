using System;
using System.Collections.Generic;
using System.Linq;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Месяц
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class Month
    {
        readonly IReadOnlyList<WeekDay> m_days;

        public IEnumerable<WeekDay> Days => m_days;

        public int Index { get; private set; }

        /// <summary>
        /// Получение даты в месяце по индексу (Индекс начинается с 1)
        /// </summary>
        /// <param name="dayIndex">Номер дня в месяце (начинается с 1)</param>
        /// <returns>Возвращает указанную дату</returns>
        /// <exception cref="InvalidOperationException">Индекс менее 1</exception>
        /// <exception cref="IndexOutOfRangeException">Индекс больше количества дней в месяце</exception>
        public DateTime this[int dayIndex]
        {
            get
            {
                if (dayIndex < 1)
                    throw new InvalidOperationException("Индекс дня должен начинаться с 1. Принятый индекс: " + dayIndex.ToString());

                if (dayIndex > DayCount)
                    throw new IndexOutOfRangeException($"Индекс дня '{dayIndex.ToString()}' больше количества дней в {Value.ToString()}");

                return new DateTime(Year, Number, dayIndex);
            }
        }

        /// <summary>
        /// Год
        /// </summary>
        public int Year { get; private set; }
        /// <summary>
        /// Имя месяца
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Значение месяца
        /// </summary>
        public Monthes Value { get; private set; }
        /// <summary>
        /// Индекс месяца в году
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Количество дней в месяце
        /// </summary>
        public int DayCount { get; private set; }

        /// <summary>
        /// Последний день текущего месяца
        /// </summary>
        public WeekDay LastDay { get { return m_days[m_days.Count - 1]; } }

        /// <summary>
        /// Последняя дата месяца
        /// </summary>
        public DateTime LastDate { get { return new DateTime(Year, Number, DayCount); } }

        /// <summary>
        /// Первы день месяца
        /// </summary>
        public WeekDay FirstDay { get { return m_days[0]; } }

        /// <summary>
        /// Первая дата месяца
        /// </summary>
        public DateTime FirstDate { get { return new DateTime(Year, Number, 1); } }

        /// <summary>
        /// Проверка на не пустой экземпляр
        /// </summary>
        public bool IsEmpty { get { return Value == Monthes.NotSet || Year <= 0; } }

        /// <summary>
        /// Возвращает новую коллекцию с индексакми дней, начиная с 1 для данного месяца (1 ... 28/29/30/31)
        /// </summary>
        public IList<int> DayIndexes
        {
            get
            {
                var list = new List<int>();

                var dayCount = GetDayCount(Value, Year) + 1;
                for (int i = 1; i < dayCount; i++)
                    list.Add(i);

                return list;
            }
        }

        public IEnumerable<string> DayNames
        {
            get
            {
                var list = new List<string>();

                string name = string.Empty;
                int dayIndex = 0;
                var dayCount = GetDayCount(Value, Year);

                for (int i = 0; i < dayCount; i++)
                {
                    switch (dayIndex)
                    {
                        case 0: name = "пн"; break;
                        case 1: name = "вт"; break;
                        case 2: name = "ср"; break;
                        case 3: name = "чт"; break;
                        case 4: name = "пт"; break;
                        case 5: name = "сб"; break;
                        case 6: name = "вс"; break;
                    }

                    list.Add(name);

                    dayIndex++;

                    if (dayIndex == 7)
                        dayIndex = 0;
                }

                return list;
            }
        }

        public Month(int year, Monthes month)
        {
            Year = year;
            Name = GetRussianName(month);
            Value = month;
            Number = (int)month;
            DayCount = GetDayCount(month, year);
            m_days = GetDays().ToList();

            Index = (Number * 10000) + year;
        }
        public Month(int year, int monthIndex) : this(year, (Monthes)monthIndex) { }

        public Month(DateTime date) : this(date.Year, (Monthes)date.Month) { }

        /// <summary>
        /// ПОлучение количества дней в конкретном месяце
        /// </summary>
        /// <param name="month">Месяц</param>
        /// <param name="year">Год. Опционально, для определения високосности</param>
        /// <returns>Возвращает количество дней в переданном месяце</returns>
        public static int GetDayCount(Monthes month, int? year)
        {
            switch (month)
            {
                case Monthes.January:
                    return 31;
                case Monthes.February:
                    if (year.HasValue)
                    {
                        if (DateTime.IsLeapYear(year.Value))
                            return 29;
                    }

                    return 28;
                case Monthes.March:
                    return 31;
                case Monthes.April:
                    return 30;
                case Monthes.May:
                    return 31;
                case Monthes.June:
                    return 30;
                case Monthes.July:
                    return 31;
                case Monthes.August:
                    return 31;
                case Monthes.September:
                    return 30;
                case Monthes.October:
                    return 31;
                case Monthes.November:
                    return 30;
                case Monthes.December:
                    return 31;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Получение количества дней в указанном периоде
        /// </summary>
        /// <param name="start">Начальный месяц</param>
        /// <param name="end">КОнечный месяц</param>
        /// <returns></returns>
        public static int GetDaysCount(Month start, Month end)
        {
            return (end.LastDate - start.FirstDate).Days + 1;
        }

        /// <summary>
        /// Получение набора месяцев между датами
        /// </summary>
        /// <param name="start">Дата начала</param>
        /// <param name="end">Дата окончания</param>
        /// <returns>Возвращает набор месяцев</returns>
        public static Month[] GetMonthes(DateTime start, DateTime end)
        {
            var count = GetMonthesCount(start, end);

            switch (count)
            {
                case 0:
                    return new Month[] { };
                case 1:
                    return new Month[] { new Month(start) };
                default:
                    var l = new List<Month>();
                    var curent = new Month(start);
                    l.Add(curent);
                    for (int i = 1; i < count; i++)
                    {
                        curent = curent.Next();
                        l.Add(curent);
                    }

                    return l.ToArray();
            }

        }

        public static Month[] GetMonthes(Month start, Month end)
        {
            return GetMonthes(new DateTime(start.Year, start.Number, 1), new DateTime(end.Year, end.Number, 1));
        }

        /// <summary>
        /// Разница в месяцах между двумя датами
        /// </summary>
        /// <param name="start">Дата начала</param>
        /// <param name="end">Дата конечная</param>
        /// <returns>Возвращает количество месяцев между датами (включительно) (округление до большего)</returns>
        public static int GetMonthesCount(DateTime start, DateTime end)
        {
            if (start.Year == end.Year)
            {
                if (start.Month == end.Month)
                    return 1;

                var res = Math.Abs(start.Month - end.Month) + 1; //+1 чтобы включительно с последним месяцем

                return res;
            }
            else
            {
                var startTail = start.Month; //количество месяцев в первом году
                var endTail = end.Month; //количество месяцев в последнем году

                var years = (end.Year - start.Year - 2); //количество полных лет между датами
                if (years < 0)
                    years = 0;

                return startTail + endTail + (years * 12);
            }
        }

        /// <summary>
        /// Получение русского имени месяца
        /// </summary>
        /// <param name="month">Месяц</param>
        /// <returns>Возвращает кирилическое имя месяца</returns>
        public static string GetRussianName(Monthes month)
        {
            switch (month)
            {
                case Monthes.January:
                    return "Январь";
                case Monthes.February:
                    return "Февраль";
                case Monthes.March:
                    return "Март";
                case Monthes.April:
                    return "Апрель";
                case Monthes.May:
                    return "Май";
                case Monthes.June:
                    return "Июнь";
                case Monthes.July:
                    return "Июль";
                case Monthes.August:
                    return "Август";
                case Monthes.September:
                    return "Сентябрь";
                case Monthes.October:
                    return "Октябрь";
                case Monthes.November:
                    return "Ноябрь";
                case Monthes.December:
                    return "Декабрь";
                default: return "Не определён";
            }
        }

        string DebugDisplay()
        {
            return (string.IsNullOrEmpty(Name) ? "UN KNOWN" : Name) + " | " + Year.ToString();
        }

        /// <summary>
        /// Получение первого дня недели в месяце
        /// </summary>
        /// <returns>Возвращает первый день недели в текущем месяце</returns>
        public DayOfWeek GetFirstDayOfWeek()
        {
            if (IsEmpty)
                return DayOfWeek.Sunday;

            return GetFirstDateOfWeek().DayOfWeek;
        }

        /// <summary>
        /// Получение первого дня в месяце
        /// </summary>
        /// <returns>Возвращает первый день в текущем месяце</returns>
        public DateTime GetFirstDateOfWeek()
        {
            return new DateTime(IsEmpty ? DateTime.Now.Year : Year, IsEmpty ? 1 : Number, 1);
        }

        public static bool IsNullOrEmpty(Month m)
        {
            if (m == null)
                return true;

            return false;
        }

        IEnumerable<WeekDay> GetDays()
        {
            List<WeekDay> days = new List<WeekDay>();

            var date = GetFirstDateOfWeek();

            var weekDay = new WeekDay(date);

            for (int i = 1; i <= DayCount; i++)
            {
                days.Add(weekDay);

                weekDay = weekDay.Next();
            }

            return days;
        }

        public string GetSqlDate(int dayNumber)
        {
            return Year.ToString() + (Number).ToString("00") + dayNumber.ToString("00");
        }

        /// <summary>
        /// Cледующий месяц
        /// </summary>
        /// <returns>Возвращает новый экземпляр месяца</returns>
        public Month Next()
        {
            if (Value == Monthes.December)
            {
                var year = Year + 1;
                return new Month(year, Monthes.January);
            }

            return new Month(Year, Value + 1);
        }

        /// <summary>
        /// Cледующий n-месяц от текущего
        /// </summary>
        /// <param name="offset">Смещения (месяцеа)</param>
        /// <returns>Возвращает месяц через n-месяце после текущего</returns>
        public Month Next(int offset)
        {
            var year = Year;

            //Раз следующий, то должен быть следующим
            //нефиг отрицательные значения передавать
            if (offset < 1)
                offset = 1;

            if (offset == 12)
                return new Month(year + 1, Value); //тот же месяц, но в следующем году

            var yCount = offset / 12; //колиечство лет
            offset = offset % 12; //значимые месяцы

            if (yCount > 0)
            {
                year = year + yCount;
                if (offset == 0)//если это тот же месяц, но через сколько-то лет
                    return new Month(year, Value);
            }

            var nextIndex = Number + offset;

            if (nextIndex < 13)
                return new Month(year, (Monthes)nextIndex);

            else //если пошёл следующий год
            {
                year++;
                offset = nextIndex - 12;
                return new Month(year, (Monthes)offset);
            }
        }

        /// <summary>
        /// Предидущий месяц от текущего
        /// </summary>
        /// <returns>Возвращает новый экземпляр предыдущего месяца</returns>
        public Month Previos()
        {
            if (Value == Monthes.January)
            {
                var year = Year - 1;
                return new Month(year, Monthes.December);
            }

            return new Month(Year, Value - 1);
        }

        /// <summary>
        /// Сравнение значений месяцев
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public bool Equals(Month m)
        {
            if (m == null)
                return false;

            return m.Year == Year && m.Value == Value;
        }

        /// <summary>
        /// Возвращает текущий месяц
        /// </summary>
        public static Month Current
        {
            get
            {
                var now = DateTime.Now;
                return new Month(now.Year, (Monthes)now.Month);
            }
        }

        /// <summary>
        /// Получение всех месяцев указанного года
        /// </summary>
        /// <param name="year">Год</param>
        /// <returns>ВОзвращает все месяцы указанного года</returns>
        public static IEnumerable<Month> GetMonthes(int year = 0)
        {
            if (year < 2000 || year > 2100)
                year = DateTime.Now.Year;

            var list = new List<Month>();

            for (int i = 1; i < 13; i++)
            {
                list.Add(new Month(year, (Monthes)i));
            }

            return list;
        }

        /// <summary>
        /// Получение всех месяцев (включительно) между указанными в рамках одного года
        /// </summary>
        /// <param name="start">Начальный месяц</param>
        /// <param name="end">Конечный месяц</param>
        /// <param name="year">Год</param>
        /// <returns></returns>
        public static IEnumerable<Month> GetMonthes(int start, int end, int year = 0)
        {
            if (year == 0)
                year = DateTime.Now.Year;

            var list = new List<Month>();
            if (start < 1)
                start = 1;

            if (end > 12)
                end = 12;

            for (int i = start; i <= end; i++)
            {
                list.Add(new Month(year, (Monthes)i));
            }

            return list;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var m = obj as Month;

            if (m != null)
                return m.Index == this.Index;
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Index;
        }

        public static bool operator >(Month m1, Month m2)
        {
            if (m1 == null || m2 == null)
                return false;

            return m1.Index > m2.Index;
        }

        public static bool operator <(Month m1, Month m2)
        {
            if (m1 == null || m2 == null)
                return false;

            return m1.Index < m2.Index;
        }

        public static bool operator >=(Month m1, Month m2)
        {
            if (m1 == null || m2 == null)
                return false;

            return m1.Index >= m2.Index;
        }

        public static bool operator <=(Month m1, Month m2)
        {
            if (m1 == null || m2 == null)
                return false;

            return m1.Index <= m2.Index;
        }

        public static bool operator ==(Month m1, Month m2)
        {
            if (ReferenceEquals(m1, m2))
                return true;

            if (ReferenceEquals(m1, null) || ReferenceEquals(m2, null))
                return false;

            return m1.Index == m2.Index;
        }

        public static bool operator !=(Month m1, Month m2)
        {
            if (ReferenceEquals(m1, m2))
                return false;

            if (ReferenceEquals(m1, null) || ReferenceEquals(m2, null))
                return true;

            return m1.Index != m2.Index;
        }
    }
}
