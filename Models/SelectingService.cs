using CarLeasingViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Сервис выбора дней
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class SelectingService : ViewModelBase, IDisposable
    {
        readonly List<WeekDay> m_daysToRemove = new List<WeekDay>();
        /// <summary>
        /// При изменении выбора
        /// </summary>
        public event Action<SelectingService> OnSelectionChanged;
        /// <summary>
        /// Множественный выбор закончился
        /// </summary>
        public event Action<SelectingService> OnSelectionFinished;

        /// <summary>
        /// Флаг наличия фильтра по дням
        /// </summary>
        public bool IsEmpty => m_SelectedDays.Count == 0;
        /// <summary>
        /// Количество выбранных дней
        /// </summary>
        public int Count => m_SelectedDays.Count;

        private SortedSet<WeekDay> m_SelectedDays = new SortedSet<WeekDay>();
        /// <summary>
        /// Возвращает или задаёт 
        /// </summary>
        public SortedSet<WeekDay> SelectedDays
        {
            get { return m_SelectedDays; }
            set
            {
                if (m_SelectedDays == value)
                    return;

                m_SelectedDays = value;

                OnPropertyChanged();
            }
        }

        private bool m_IsSelecting;
        /// <summary>
        /// Возвращает или задаёт флаг, что выбор ещё не закончен
        /// </summary>
        public bool IsSelecting
        {
            get { return m_IsSelecting; }
            set
            {
                if (m_IsSelecting == value)
                    return;

                m_IsSelecting = value;

                OnPropertyChanged();
            }
        }

        public void Append(WeekDay day)
        {
            if (day == null)
            {
                return;
            }

            var day_added = m_SelectedDays.Contains(day);
            if (!day_added)
            {
                SelectInternal(day, true);
            }

            SelectionChanged();

            SelectionFinished();
        }

        public void Select(WeekDay day)
        {
            Select(day, null);
        }

        public void Select(WeekDay from, WeekDay to)
        {
            if (from == null && to == null)
                return;

            ClearSelectedDays();

            if (to == null)
            {
                SelectInternal(from, true);
            }
            else
            {
                var days = GetDaysRange(from, to);

                SelectInternal(days, true);
            }

            SelectionChanged();

            SelectionFinished();
        }

        public void PreSelect(WeekDay day)
        {
            if (day == null)
                return;

            SelectInternal(day, true);

            SelectionChanged();
        }

        public void Remove(WeekDay day)
        {
            Remove(day, null);
        }

        public void Remove(WeekDay from, WeekDay to)
        {
            if (to == null)
            {
                if (from == null)
                {
                    throw new ArgumentNullException("Попытка удаления из фильтров");
                }
                else
                {
                    SelectInternal(from, false);
                }
            }
            else
            {
                var days = GetDaysRange(from, to);

                SelectInternal(days, false);
            }

            SelectionChanged();

            if (m_SelectedDays.Count == 0)
                SelectionFinished();
        }

        public void PreReset()
        {
            if (m_SelectedDays.Count > 0)
            {
                m_daysToRemove.Clear();

                m_daysToRemove.AddRange(m_SelectedDays);

                foreach (var day in m_daysToRemove)
                {
                    m_SelectedDays.Remove(day);

                    day.Selected = false;
                }
            }
        }

        public void Reset()
        {
            if (m_SelectedDays.Count > 0)
            {
                ClearSelectedDays();

                SelectedDays = new SortedSet<WeekDay>();

                SelectionChanged();
            }

            SelectionFinished();
        }

        public void SelectionFinished()
        {
            if (IsSelecting == true)
                IsSelecting = false;

            if (OnSelectionFinished != null)
                OnSelectionFinished(this);
        }

        void SelectionChanged()
        {
            SelectedDays = new SortedSet<WeekDay>(m_SelectedDays);

            if (OnSelectionChanged != null)
                OnSelectionChanged.Invoke(this);
        }

        List<WeekDay> GetDaysRange(WeekDay from, WeekDay to)
        {
            var current = from;

            var list = new List<WeekDay>();

            while (current.Index <= to.Index)
            {
                list.Add(current);

                current = current.Next();
            }

            return list;
        }

        void SelectInternal(WeekDay day, bool select)
        {
            SelectInternal(new[] { day }, select);
        }

        void SelectInternal(IEnumerable<WeekDay> days, bool select)
        {
            if (days == m_SelectedDays)
            {
                // чтобы небыло проблем при удалении из коллекции во время обхода
                days = new List<WeekDay>(days);
            }

            foreach (var day in days)
            {
                if (day.Selected != select)
                {
                    day.Selected = select;

                    if (select)
                    {
                        m_SelectedDays.Add(day);
                    }
                    else
                    {
                        m_SelectedDays.Remove(day);

                        day.Hightlighted = false;
                    }
                }
            }
        }

        void ClearSelectedDays()
        {
            if (m_SelectedDays == null || m_SelectedDays.Count == 0)
                return;

            SelectInternal(m_SelectedDays, false);
        }

        string DebugDisplay()
        {
            string message = "EMPTY";

            if (m_SelectedDays.Count > 0)
            {
                if (m_SelectedDays.Count == 1)
                    message = m_SelectedDays.First().Date.ToString("yyyy-MM-dd");
                else
                {
                    message = $"{m_SelectedDays.First().Date.ToString("yyyy-MM-dd")} - {m_SelectedDays.Last().Date.ToString("yyyy-MM-dd")} | Count: {m_SelectedDays.Count}";
                }
            }

            if (IsSelecting)
                message += " | IsSelecting...";

            return message;
        }

        public void Dispose()
        {
            m_SelectedDays?.Clear();
            m_SelectedDays?.Clear();
        }
    }
}
