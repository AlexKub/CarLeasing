using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CarLeasingViewer.Controls.LeasingChartManagers
{
    public delegate void RowLayoutDrawed(CanvasRowLayoutDrawManager.RowLayout layout);

    /// <summary>
    /// Управление отрисовкой "строк" на графике
    /// </summary>
    public class CanvasRowLayoutDrawManager : CanvasDrawManager
    {
        /*
         * Для реализации подсветки строк при выборе/наведении мышкой
         * отрисовываем на графике невидимые прямоугольники во всю строку для каждой строки - подложки строк
         * 
         * при изменении подсветки будем изменять Background этих прямоугольников
         */

        /// <summary>
        /// Индексы в Dictionary в порядке добавления
        /// </summary>
        List<int> m_indexes = new List<int>(); //отвязываемся от порядка добавления индексов
        Dictionary<int, RowLayout> m_bars = new Dictionary<int, RowLayout>();

        /// <summary>
        /// Прозрачная кисть для Layout'a
        /// </summary>
        public static readonly Brush DefaultBrush = Brushes.Transparent;

        public event RowLayoutDrawed RowLayoutDrawed;

        /// <summary>
        /// Высота строки
        /// </summary>
        public double RowHeight { get; set; }

        /// <summary>
        /// Отрисованные строки
        /// </summary>
        public IEnumerable<RowLayout> Rows { get { return m_bars.Values; } }

        /// <summary>
        /// Получение подложки строки по указанному индексу
        /// </summary>
        /// <param name="index">Индекс строки на графике, начиная с 0</param>
        /// <returns>Возвращает соответствующую подложку или null</returns>
        public RowLayout this[int index] { get { return m_bars.ContainsKey(index) ? m_bars[index] : null; } }

        public CanvasRowLayoutDrawManager(LeasingChart chart) : base(chart)
        {
            DefaultBrush.Freeze();
        }

        /// <summary>
        /// Отрисовка Layout'a у строки графика
        /// </summary>
        /// <param name="rowIndex">Индекс строки</param>
        /// <returns>Возвращает DrawingVisual, которым был отрисован Layout</returns>
        public DrawingVisual DrawRowLayout(int rowIndex)
        {
            RowLayout rl = null;

            if (m_bars.ContainsKey(rowIndex))
                rl = m_bars[rowIndex];
            else
            {
                rl = new RowLayout();
                rl.RowIndex = rowIndex;
                m_bars.Add(rowIndex, rl);
                m_indexes.Add(rowIndex);
            }

            return DrawLayoutRect(rl);
        }

        DrawingVisual DrawLayoutRect(RowLayout rl)
        {
            var verticalOffset = rl.RowIndex * RowHeight;

            Rect rect = new Rect(0, verticalOffset, Canvas.ActualWidth, RowHeight);
            rl.Rectangle = rect;

            var dv = new DrawingVisual();
            rl.Visual = dv;
            var dc = dv.RenderOpen();

            dc.DrawRectangle(DefaultBrush, null, rect);
            dc.Close();

            RowLayoutDrawed?.Invoke(rl);

            return dv;
        }

        /// <summary>
        /// Получение Layout'a, содержащего переданную точку
        /// </summary>
        /// <param name="p">Искомая точка</param>
        /// <returns>Возвращает содержащий Layout или null</returns>
        public RowLayout Contains(Point p)
        {
            if (m_indexes.Count == 0)
                return null;

            var rowIndex = (int)(p.Y / RowHeight) - 1;

            if (rowIndex < 0)
                rowIndex = 0;

            if (rowIndex < m_indexes.Count)
            {
                if (m_bars[m_indexes[rowIndex]].Rectangle.Contains(p))
                    return m_bars[m_indexes[rowIndex]];
            }
            else
                return null;

            rowIndex++;

            if (rowIndex < m_indexes.Count)
                if (m_bars[m_indexes[rowIndex]].Rectangle.Contains(p))
                    return m_bars[m_indexes[rowIndex]];

            return null;
        }

        public void Clear()
        {
            if (m_bars != null)
                m_bars.Clear();

            if (m_indexes != null)
                m_indexes.Clear();
        }

        public override void Dispose()
        {
            Clear();

            base.Dispose();
        }

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
}
