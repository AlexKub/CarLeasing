using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CarLeasingViewer.Controls.LeasingChartManagers
{
    public delegate void ColumnLayoutDrawed(ColumnLayout layout);

    /// <summary>
    /// Управление отрисовкой "строк" на графике
    /// </summary>
    public class CanvasColumnLayoutDrawManager : CanvasDrawManager
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
        Dictionary<int, ColumnLayout> m_bars = new Dictionary<int, ColumnLayout>();

        /// <summary>
        /// Прозрачная кисть для Layout'a
        /// </summary>
        public static readonly Brush DefaultBrush = Brushes.Transparent;

        public event ColumnLayoutDrawed RowLayoutDrawed;

        /// <summary>
        /// Высота строки
        /// </summary>
        public double ColumnWidth { get; set; }

        /// <summary>
        /// Отрисованные строки
        /// </summary>
        public IEnumerable<ColumnLayout> Columns { get { return m_bars.Values; } }

        /// <summary>
        /// Получение подложки строки по указанному индексу
        /// </summary>
        /// <param name="index">Индекс строки на графике, начиная с 0</param>
        /// <returns>Возвращает соответствующую подложку или null</returns>
        public ColumnLayout this[int index] { get { return m_bars.ContainsKey(index) ? m_bars[index] : null; } }

        public CanvasColumnLayoutDrawManager(LeasingChart chart) : base(chart)
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
            var cl = new ColumnLayout();
            cl.ColumnIndex = rowIndex;
            m_indexes.Add(rowIndex);

            return DrawLayoutRect(cl);
        }

        DrawingVisual DrawLayoutRect(ColumnLayout cl)
        {
            var verticalOffset = cl.ColumnIndex * ColumnWidth;

            // TODO
            Rect rect = new Rect(0, verticalOffset, Canvas.ActualWidth, ColumnWidth);
            cl.Rectangle = rect;

            var dv = new DrawingVisual();
            cl.Visual = dv;
            var dc = dv.RenderOpen();

            dc.DrawRectangle(DefaultBrush, null, rect);
            dc.Close();

            RowLayoutDrawed?.Invoke(cl);

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

            var row = Canvas.RowManager.GetRowByPoint(p);

            if (row != null)
                return row.RowLayout;

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
    }
}
