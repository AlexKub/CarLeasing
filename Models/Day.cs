using System;
using System.Windows.Media;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// День недели
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Name}")]
    public class Day : ViewModels.ViewModelBase, IComparable<Day>
    {
        /// <summary>
        /// Короткое имя
        /// </summary>
        public string ShortName { get { return GetShortName(Type); } }

        /// <summary>
        /// Полное имя
        /// </summary>
        public string Name { get { return GetName(Type); } }

        /// <summary>
        /// Тип дня недели
        /// </summary>
        public DayOfWeek Type { get; set; }

        /// <summary>
        /// Дата месяца
        /// </summary>
        public int Index { get; private set; }

        private bool pv_Selected;
        /// <summary>
        /// Возвращает или задаёт флаг, что день был выбран мышью
        /// </summary>
        public bool Selected { get { return pv_Selected; } set { if (pv_Selected != value) { pv_Selected = value; OnPropertyChanged(); } } }

        public Day(int index, DayOfWeek type)
        {
            Index = index;
            Type = type;
        }


        public static string GetShortName(DayOfWeek type)
        {
            switch(type)
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

        public static string GetName(DayOfWeek type)
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

        public int CompareTo(Day other)
        {
            if (other == null)
                return 1;

            return other.Index > Index ? -1 : other.Index == Index ? 0 : 1;
        }
    }
}
