using System;
using System.Windows.Media;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// День недели
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class WeekDay : ViewModels.ViewModelBase, IComparable<WeekDay>
    {
        /// <summary>
        /// Короткое имя
        /// </summary>
        public string ShortName { get { return DayOfWeek.GetShortName(); } }

        /// <summary>
        /// Полное имя
        /// </summary>
        public string Name { get { return DayOfWeek.GetName(); } }

        /// <summary>
        /// Тип дня недели
        /// </summary>
        public DayOfWeek DayOfWeek { get; set; }
        /// <summary>
        /// Номер месяца
        /// </summary>
        public int Number { get; private set; }
        /// <summary>
        /// Дата месяца
        /// </summary>
        public int Index { get; private set; }
        /// <summary>
        /// Дата
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Флаг снятия выбора с дня (поддержка клика)
        /// </summary>
        public bool IsUnSelecting { get; set; }

        private bool pv_Selected;
        /// <summary>
        /// Возвращает или задаёт флаг, что день был выбран мышью
        /// </summary>
        public bool Selected { get { return pv_Selected; } set { if (pv_Selected != value) { pv_Selected = value; OnPropertyChanged(); } } }

        private bool m_Hightlighted;
        /// <summary>
        /// Возвращает или задаёт флаг, что день должен быть подсвечен
        /// </summary>
        public bool Hightlighted
        {
            get { return m_Hightlighted; }
            set
            {
                if (m_Hightlighted == value)
                    return;

                m_Hightlighted = value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// День недели
        /// </summary>
        /// <param name="date">Дата</param>
        public WeekDay(DateTime date)
        {
            Number = date.Day;
            Index = GetIndex(date);
            DayOfWeek = date.DayOfWeek;
            Date = date;
        }

        static int GetIndex(DateTime date)
        {
            return date.Year + (date.Day * 10000);
        }

        public WeekDay Next()
        {
            return new WeekDay(Date.AddDays(1).Date);
        }

        public int CompareTo(WeekDay other)
        {
            if (other == null)
                return -1;

            if (other.Index == this.Index)
                return 0;

            if (other.Index > this.Index)
                return 1;

            return -1;
        }

        public override int GetHashCode() => Index;

        string DebugDisplay() => $"{Index} | {Date.ToString("yyyy-MM-dd")} | {ShortName}";
    }
}
