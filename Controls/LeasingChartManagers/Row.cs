using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace CarLeasingViewer.Controls.LeasingChartManagers
{
    /// <summary>
    /// Условная строка на графике
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugerDisplay()}")]
    public class Row : IDisposable
    {
        List<CanvasBarDrawManager.BarData> m_Bars = new List<CanvasBarDrawManager.BarData>();

        /// <summary>
        /// Индекс строки (начиная с 0)
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Данные о машине
        /// </summary>
        public CarModel Car { get; set; }

        /// <summary>
        /// Layout строки
        /// </summary>
        public RowLayout RowLayout { get; set; }

        /// <summary>
        /// Данные полосок, принадлежащих текущей строке графика
        /// </summary>
        public IReadOnlyList<CanvasBarDrawManager.BarData> Bars { get { return m_Bars; } }

        /// <summary>
        /// Комментарий
        /// </summary>
        public CarCommentModel Comment { get; set; }

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

        public Row(int i) { Index = i; }

        public void Add(CanvasBarDrawManager.BarData bar)
        {
            m_Bars.Add(bar);
        }
        public void AddRange(IEnumerable<CanvasBarDrawManager.BarData> bars)
        {
            m_Bars.AddRange(bars);
        }

        /// <summary>
        /// Подсветка строки
        /// </summary>
        /// <param name="brush">Кисть подсветки</param>
        public void Hightlight(Brush brush)
        {
            if (Car != null)
                Car.HightlightBrush = brush;
            if (RowLayout != null)
                RowLayout.HightlightBrush = brush;
            if (Comment != null)
                Comment.HightlightBrush = brush;
        }

        /// <summary>
        /// Простановка флага "подсвечено"
        /// </summary>
        /// <param name="hightlight">Флаг</param>
        void SetHighlightFlag(bool hightlight)
        {
            if (Car != null)
                Car.Hightlighted = hightlight;
            if (RowLayout != null)
                RowLayout.Hightlighted = hightlight;
            if (Comment != null)
                Comment.Hightlighted = hightlight;
        }

        public void Clear()
        {
            Car = null;
            RowLayout = null;
            Comment = null;
            m_Bars.Clear();
        }

        public void Dispose()
        {
            Clear();
        }

        string DebugerDisplay()
        {
            return Index.ToString() + " | " + (Car == null ? "NULL" : string.IsNullOrEmpty(Car.Text) ? "EMPTY" : Car.Text);
        }
    }
}
