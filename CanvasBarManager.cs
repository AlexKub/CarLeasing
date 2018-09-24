using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

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
        /// Ширина колонки дня
        /// </summary>
        public double DayColumnWidth { get; set; }

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
                bd.HorizontalOffset = barModel.DayOffset + GetMonthOffset(barModel);
                bd.BarModel = barModel;
            }

            var b = DrawBorder(bd);
            bd.Border = b;
            bd.Glyphs = DrawText(bd);
            b.Child = bd.Glyphs;
        }

        Border DrawBorder(BarData bd)
        {
            var b = new Border();
            b.BorderBrush = BorderBrush;
            b.SnapsToDevicePixels = true;
            b.Background = BackgroundBrush;
            b.Height = RowHeight;
            b.Width = bd.BarModel.Width;
            Panel.SetZIndex(b, Z_Indexes.BarIndex);

            var lineNumber = bd.BarModel.RowIndex + 1;

            Canvas.Children.Add(b);

            Canvas.SetTop(b, bd.BarModel.RowIndex * RowHeight);
            Canvas.SetLeft(b, bd.HorizontalOffset);

            return b;
        }

        Glyphs DrawText(BarData bd)
        {
            var g = new Glyphs();

            var barModel = bd.BarModel;
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
        /// Смещение по количеству дней в предидущих месяцах
        /// </summary>
        /// <param name="barModel">Данные текущей полоски месяца</param>
        /// <returns>Возвращает готовой смещение по месяцу или 0</returns>
        double GetMonthOffset(LeasingBarModel barModel)
        {
            /*
             * расчёт месячного смещения
             * 
             * Пример (о чём речь):
             * если сейчас (в переданной модели) месяц март,
             * а в шапке перед этим месяцем вставены ещё сколько-то месяцев
             * то необходимо к текущему смещению добавить ещё количество дней в этих месмяцах
             * 
             * количество дней в предидущих месяцах и возвращает этот метод
             */
            if (barModel == null)
                return 0;

            var offset = 0d;

            if (barModel.Month != null)
            {
                if (barModel.Month.Previous != null)
                {
                    var prev = barModel.Month.Previous;

                    while (prev != null)
                    {
                        if (prev.Month != null)
                            offset += (prev.Month.DayCount * DayColumnWidth);

                        prev = prev.Previous;
                    }
                }
            }

            return offset;
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
            /// Отступ сверху для текущего элемента
            /// </summary>
            public double VerticalOffset { get; set; }
            /// <summary>
            /// Отступ сдева для текущего элемента
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

            public LeasingBarModel BarModel { get; set; }

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
                    BarModel = null;
                }
            }
        }
    }
}
