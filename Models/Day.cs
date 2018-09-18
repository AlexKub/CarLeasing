using System;
using System.Windows.Media;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// День недели
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Name}")]
    public class Day : ViewModels.ViewModelBase
    {
        public static Brush DefaultBackground { get { return Brushes.White; } }

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

        private Brush pv_Background;
        /// <summary>
        /// Возвращает или задаёт подсветку для Дней
        /// </summary>
        public Brush Background { get { return pv_Background; } set { if (pv_Background != value) { pv_Background = value; OnPropertyChanged(); } } }

        public Day(int index, DayOfWeek type)
        {
            Index = index;
            Type = type;
            Background = DefaultBackground;
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
    }
}
