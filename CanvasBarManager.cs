using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CarLeasingViewer
{
    /// <summary>
    /// Управление отрисовкой полосок занятости авто на Canvas
    /// </summary>
    public class CanvasBarManager : CanvasManager
    {
        GlyphTypeface m_glyphType;

        Typeface m_Typeface;

        /// <summary>
        /// Шрифт для расчёта размеров текста. Кешируем один раз, чтобы не генерировать кучу экземпляров.
        /// </summary>
        System.Drawing.Font m_drawingFont;
        /// <summary>
        /// Кисть для заливки границы
        /// </summary>
        public Brush BorderBrush { get; set; }

        /// <summary>
        /// Кисть для заливки фона полосок
        /// </summary>
        public Brush BackgroundBrush { get; set; }

        public Brush TextBrush { get; set; }

        private FontFamily m_FontFamily;
        /// <summary>
        /// Шрифт
        /// </summary>
        public FontFamily FontFamily
        {
            get { return m_FontFamily; }
            set
            {
                if (value != m_FontFamily)
                {
                    m_FontFamily = value;
                    m_glyphType = GetGlyphTypeface();
                }
            }
        }

        private double m_FontSize;
        /// <summary>
        /// Размер шрифта
        /// </summary>
        public double FontSize
        {
            get { return m_FontSize; }
            set
            {
                m_FontSize = value;

                SetDrawingFont();
            }
        }

        public double RowHeight { get; set; }

        /// <summary>
        /// Ширина колонки дня
        /// </summary>
        public double DayColumnWidth { get; set; }

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
                m_bars.Add(barModel, bd);
            }

            var b = DrawBorder(bd);
            bd.Border = b;
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

        public void DrawText(DrawingContext dc)
        {
            //проверяем инициализацию для интерфйса шрифта
            //инициализирован при заполнении шрифта (this.FontFamily)
            if (m_glyphType == null)
                m_glyphType = GetGlyphTypeface();

            foreach (var item in m_bars.Values)
            {
                if (!item.TextDrawed)
                {
                    DrawText(item, dc);
                    item.TextDrawed = true;
                }
            }
        }

        /// <summary>
        /// Отрисовка текста на панелях
        /// </summary>
        /// <param name="bd">Данные для отрисовки</param>
        /// <returns>Возвращает</returns>
        void DrawText(BarData bd, DrawingContext dc)
        {
            //var g = new Glyphs();
            //
            //var barModel = bd.BarModel;
            //g.UnicodeString = barModel.Leasing?.Title ?? "NO TITLE";
            //g.Height = RowHeight;
            //g.Width = (barModel.Leasing.DateEnd - barModel.Leasing.DateStart).Days * barModel.DayColumnWidth;
            //g.Fill = Brushes.Black;
            //g.Fill.Freeze();
            //g.OriginX = 0;
            //g.OriginY = ((RowHeight - FontSize) / 2) + FontSize;
            //g.FontRenderingEmSize = FontSize;
            //g.FontUri = new Uri(@"C:\WINDOWS\Fonts\TIMES.TTF");
            //
            //return g;

            //взято из https://smellegantcode.wordpress.com/2008/07/03/glyphrun-and-so-forth/
            string text = bd?.BarModel?.Leasing?.Title ?? "NO TITLE";
            double fontSize = FontSize;

            ushort[] glyphIndexes = new ushort[text.Length];
            double[] advanceWidths = new double[text.Length];

            double totalWidth = 0;

            for (int n = 0; n < text.Length; n++)
            {
                ushort glyphIndex = m_glyphType.CharacterToGlyphMap[text[n]];
                glyphIndexes[n] = glyphIndex;

                double width = m_glyphType.AdvanceWidths[glyphIndex] * fontSize;
                advanceWidths[n] = width;

                totalWidth += width;
            }

            //рамка текста (ширина/высота)
            //расчитываем ширину / высоту перед отрисовкой для центровки текста
            FormattedText ft = new FormattedText(text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, m_Typeface, fontSize, TextBrush);
            var textRect = new Size(ft.Width, ft.Height); //System.Windows.Forms.TextRenderer.MeasureText(text, m_drawingFont); 
            var x = bd.HorizontalOffset //отступ по горизонтали (дни)
                + ((bd.Border.Width - textRect.Width) / 2); //центровка на полоске

            var y = bd.VerticalOffset //отступ по вертикали (строки)
                + (bd.Border.Height > FontSize ? ((bd.Border.Height - FontSize) / 2) : 0); //центровка текста по вертикали

            //точные координаты начала текста на Canvas
            Point origin = new Point(x, y);

            //объект отрисовки текста
            //GlyphRun glyphRun = new GlyphRun(m_glyphType, 0, false, fontSize,
            //    glyphIndexes, origin, advanceWidths, null, null, null, null,
            //    null, null);

            
            
            dc.DrawText(ft, origin);
        }

        public CanvasBarManager(Canvas canvas) : base(canvas)
        {
            //цвет текста по умолчанию
            TextBrush = Brushes.Black;
            TextBrush.Freeze();
        }

        public override void Dispose()
        {
            foreach (var item in m_bars.Values)
            {
                item.Clear();
            }

            m_bars.Clear();
            m_bars = null;

            if(m_drawingFont != null)
            {
                m_drawingFont.Dispose();
                m_drawingFont = null;
            }

            m_glyphType = null;

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

        GlyphTypeface GetGlyphTypeface()
        {

            m_Typeface = new Typeface(FontFamily ?? new FontFamily("Times New Roman"),
                                FontStyles.Normal,
                                FontWeights.Normal,
                                FontStretches.Normal);
            
            
            GlyphTypeface glyphTypeface;
            if (!m_Typeface.TryGetGlyphTypeface(out glyphTypeface))
                throw new InvalidOperationException("No glyphtypeface found");

            return glyphTypeface;
        }

        /// <summary>
        /// Простановка кешируемого шрифта для просчёта размеров текста
        /// </summary>
        void SetDrawingFont()
        {
            //при простановке 0 пробросит исключение
            if (m_FontSize > 0d)
            {
                if (m_drawingFont != null)
                    m_drawingFont.Dispose();

                m_drawingFont = new System.Drawing.Font(new System.Drawing.FontFamily(FontFamily.Source), (float)m_FontSize);
            }
        }

        /// <summary>
        /// Данные для отрисовки полоски
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

            public LeasingBarModel BarModel { get; set; }

            /// <summary>
            /// Флаг отрисовки линии
            /// </summary>
            public bool Drawed { get { return Border != null; } }

            /// <summary>
            /// Флаг отрисовки текста для данной полоски
            /// </summary>
            public bool TextDrawed { get; set; }

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
                    BarModel = null;
                }
            }
        }
    }
}
