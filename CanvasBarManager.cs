using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using CarLeasingViewer.Models;

namespace CarLeasingViewer
{
    /// <summary>
    /// Управление отрисовкой полосок занятости авто на Canvas
    /// </summary>
    public class CanvasBarManager : CanvasManager
    {
        /// <summary>
        /// Кисть для заливки границы
        /// </summary>
        public Brush BorderBrush { get; set; }

        /// <summary>
        /// Кисть для заливки фона полосок
        /// </summary>
        public Brush BackgroundBrush { get; set; }

        /// <summary>
        /// Шрифт
        /// </summary>
        public FontFamily Font { get; set; }

        /// <summary>
        /// Размер шрифта
        /// </summary>
        public double FontSize { get; set; }

        public double RowHeight { get; set; }

        /// <summary>
        /// Индекс первого месяца, с которого начинаем отсчёт
        /// </summary>
        public int FirstMonthIndex { get; set; }

        Dictionary<LeasingBarModel, BarData> m_bars = new Dictionary<LeasingBarModel, BarData>();

        public void DrawBar(LeasingBarModel barModel)
        {
            BarData bd = null;

            if (m_bars.ContainsKey(barModel))
                bd = m_bars[barModel];
            else
            {
                bd = new BarData(this);

                var lineNumber = barModel.RowIndex + 1;
                bd.VerticalOffset = lineNumber * RowHeight;
                bd.HorizontalOffset = barModel.DayOffset;
            }

            var b = DrawBorder(barModel);
            bd.Border = b;
            bd.Glyphs = DrawText(barModel);
            b.Child = bd.Glyphs;
        }

        Border DrawBorder(LeasingBarModel barModel)
        {
            var b = new Border();
            b.BorderBrush = BorderBrush;
            b.SnapsToDevicePixels = true;
            b.Background = BackgroundBrush;
            b.Height = RowHeight;
            b.Width = barModel.Width;
            Panel.SetZIndex(b, Z_Indexes.BarIndex);

            var lineNumber = barModel.RowIndex + 1;

            Canvas.Children.Add(b);

            Canvas.SetTop(b, barModel.RowIndex * RowHeight);
            Canvas.SetLeft(b, barModel.DayOffset);

            return b;
        }

        Glyphs DrawText(LeasingBarModel barModel)
        {
            var g = new Glyphs();

            g.UnicodeString = barModel.Leasing?.Title ?? "NO TITLE";
            g.Height = RowHeight;
            g.Width = (barModel.Leasing.DateEnd - barModel.Leasing.DateStart).Days * barModel.DayColumnWidth;
            g.Fill = Brushes.Black;
            g.Fill.Freeze();
            g.OriginX = 0;
            g.OriginY = ((RowHeight - FontSize) / 2) + FontSize;
            g.FontRenderingEmSize = FontSize;
            g.FontUri = new Uri(@"C:\WINDOWS\Fonts\TIMES.TTF");

            return g;
        }

        public CanvasBarManager(Canvas canvas) : base(canvas) { }

        public override void Dispose()
        {
            foreach (var item in m_bars.Values)
            {
                item.Clear();
            }

            m_bars.Clear();
            m_bars = null;

            base.Dispose();
        }

        /// <summary>
        /// Динамические данные для отрисовки строки
        /// </summary>
        class BarData
        {
            CanvasBarManager m_manager;
            /// <summary>
            /// Индекс строки на графике
            /// </summary>
            public int Index { get; set; }
            /// <summary>
            /// Актуальная высота одного из контролов на строек
            /// </summary>
            public double VerticalOffset { get; set; }
            /// <summary>
            /// Актуальная высота одного из контролов на строек
            /// </summary>
            public double HorizontalOffset { get; set; }

            /// <summary>
            /// Отрисованная граница
            /// </summary>
            public Border Border { get; set; }
            /// <summary>
            /// Отрисованный текст
            /// </summary>
            public Glyphs Glyphs { get; set; }

            /// <summary>
            /// Флаг отрисовки линии
            /// </summary>
            public bool Drawed { get { return Border != null; } }


            public BarData(CanvasBarManager manager)
            {
                m_manager = manager;
            }

            /// <summary>
            /// Удаление линии
            /// </summary>
            public void Clear()
            {
                if (Drawed)
                {
                    m_manager.Canvas.Children.Remove(Border);
                    Border = null;
                    Glyphs = null;
                }
            }
        }
    }
}
