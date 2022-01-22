using CarLeasingViewer.Models;
using System;
using System.Windows.Media;

namespace CarLeasingViewer.Controls.LeasingChartManagers
{
    /// <summary>
    /// Условная колонка на графике
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugerDisplay()}")]
    public class Column : IDisposable
    {
        /// <summary>
        /// Индекс колонки (начиная с 0)
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Данные о дне
        /// </summary>
        public WeekDay Day { get; set; }

        /// <summary>
        /// Layout колонки
        /// </summary>
        public ColumnLayout ColumnLayout { get; set; }

        HightlightManager.HightlightAction m_hState;
        /// <summary>
        /// Текущее состояние подсветки строки
        /// </summary>
        public HightlightManager.HightlightAction HightlightState
        {
            get { return m_hState; }
            set
            {
                m_hState = value;
                SetHighlightFlag(value != HightlightManager.HightlightAction.None);
            }
        }

        /// <summary>
        /// Флаг, что строка была выбрана пользователем
        /// </summary>
        public bool Selected { get; set; }

        public Column(int i) { Index = i; }

        /// <summary>
        /// Подсветка клонки
        /// </summary>
        /// <param name="brush">Кисть подсветки</param>
        public void Hightlight(Brush brush)
        {
            //if (Day != null)
            //    Day.HightlightBrush = brush;

            if (ColumnLayout != null)
                ColumnLayout.HightlightBrush = brush;
        }

        /// <summary>
        /// Простановка флага "подсвечено"
        /// </summary>
        /// <param name="hightlight">Флаг</param>
        void SetHighlightFlag(bool hightlight)
        {
            if (Day != null)
                Day.Hightlighted = hightlight;
            if (ColumnLayout != null)
                ColumnLayout.Hightlighted = hightlight;
        }

        public void Clear()
        {
            Day = null;
            ColumnLayout = null;
        }

        public void Dispose()
        {
            Clear();
        }

        string DebugerDisplay()
        {
            return Index.ToString() + " | " + (Day == null ? "NULL" : Day.Date.ToString());
        }
    }
}
