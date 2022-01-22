using System.Windows;
using System.Windows.Media;

namespace CarLeasingViewer.Controls.LeasingChartManagers
{
    /// <summary>
    /// "Подложка" строк на графике
    /// </summary>
    public class RowLayout : Interfaces.IHightlightable
    {
        /*
         * Данные невидимого прямоугольника, отрисованного на всю строку
         * через него реализуем подсветку выбранной строки
         */

        /// <summary>
        /// Индекс строки
        /// </summary>
        public int RowIndex { get; set; }
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
