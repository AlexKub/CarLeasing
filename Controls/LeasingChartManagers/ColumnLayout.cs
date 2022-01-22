using System.Windows;
using System.Windows.Media;

namespace CarLeasingViewer.Controls.LeasingChartManagers
{
    /// <summary>
    /// "Подложка" колонок на графике
    /// </summary>
    public class ColumnLayout : Interfaces.IHightlightable
    {
        /*
         * 
         * Данные невидимого прямоугольника, отрисованного на всю высоту
         * через него реализуем подсветку выбранной колонки
         * 
         */

        /// <summary>
        /// Индекс колонки
        /// </summary>
        public int ColumnIndex { get; set; }
        /// <summary>
        /// Layout-прямоугольник
        /// </summary>
        public Rect Rectangle { get; set; }

        /// <summary>
        /// Отрисовщик
        /// </summary>
        public DrawingVisual Visual { get; set; }

        void SetBackground(Brush brush)
        {
            var dc = Visual.RenderOpen();
            dc.DrawRectangle(brush, null, Rectangle);
            dc.Close();
        }

        #region IHightlightable

        Brush m_brush;
        public Brush HightlightBrush
        {
            get { return m_brush; }
            set
            {
                m_brush = value;
                SetBackground(m_brush);
            }
        }
        public bool Hightlighted { get; set; }

        #endregion
    }
}
