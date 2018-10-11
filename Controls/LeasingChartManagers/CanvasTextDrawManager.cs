using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;


namespace CarLeasingViewer.Controls.LeasingChartManagers
{
    /// <summary>
    /// Управление отрисовкой текста на Canvas
    /// </summary>
    public class CanvasTextDrawManager : CanvasDrawManager
    {
        GlyphTypeface m_glyphType;

        Typeface m_Typeface;

        /// <summary>
        /// Шрифт для расчёта размеров текста. Кешируем один раз, чтобы не генерировать кучу экземпляров.
        /// </summary>
        System.Drawing.Font m_drawingFont;

        Dictionary<LeasingElementModel, BarData> m_bars = new Dictionary<LeasingElementModel, BarData>();

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
                    if (m_drawingFont != null)
                        SetDrawingFont();
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

        /// <summary>
        /// Загрузка набора для отрисовки
        /// </summary>
        /// <param name="data">Данные для отрисовки</param>
        public void Load(IEnumerable<LeasingElementModel> data)
        {
            foreach (var item in data)
            {
                //создаём модель данных для отрисовки текста
                var bd = new BarData(this) { Index = item.RowIndex, BarModel = item };

                //добавляем связь между моделью отрисовки и моделью элемента
                m_bars.Add(item, bd);

                SetOffset(bd);
            }
        }

        /// <summary>
        /// Простановка смещения для текста
        /// </summary>
        /// <param name="bd"></param>
        void SetOffset(BarData bd)
        {
            /*
             * Простановка смещения для текста на основе
             * параметров прямоугольника (Border), 
             * отрисованного и расчитанного другим менеджером
             * 
             */
            var border = Canvas.BarManager[bd.BarModel];
            if (border != null)
            {
                bd.Border = border.Border;
                bd.HorizontalOffset = border.HorizontalOffset;
                bd.VerticalOffset = border.VerticalOffset;
            }
        }

        public CanvasTextDrawManager(LeasingChart canvas) : base(canvas)
        {
            //цвет текста по умолчанию
            TextBrush = Brushes.Black;
            TextBrush.Freeze();
        }

        public DrawingVisual DrawText(LeasingElementModel model)
        {
            //проверяем инициализацию для интерфейса шрифта
            //инициализирован при заполнении шрифта (this.FontFamily)
            //if (m_glyphType == null)
            //    m_glyphType = GetGlyphTypeface();
            //

            BarData bd = null;
            DrawingVisual dv = null;

            if (m_bars.ContainsKey(model))
                bd = m_bars[model];
            else
            {
                bd = new BarData(this) { Index = model.RowIndex, BarModel = model };
                m_bars.Add(model, bd);
                SetOffset(bd);
            }

            //если ещё не проставлены смещения
            if (bd.Border == null)
                SetOffset(bd);

            dv = DrawText(bd);
            bd.TextDrawed = true;

            return dv;
        }

        /// <summary>
        /// Отрисовка текста на панелях
        /// </summary>
        /// <param name="bd">Данные для отрисовки</param>
        /// <returns>Возвращает</returns>
        DrawingVisual DrawText(BarData bd)
        {
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
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            FormattedText ft = new FormattedText(text, culture, FlowDirection.LeftToRight, m_Typeface, fontSize, TextBrush);
            if (bd.Border.Width - 4 < ft.Width)
                ft = new FormattedText("...", culture, ft.FlowDirection, m_Typeface, fontSize, TextBrush);

            var textRect = new Size(ft.Width, ft.Height); //System.Windows.Forms.TextRenderer.MeasureText(text, m_drawingFont); 
            var x = bd.HorizontalOffset //отступ по горизонтали (дни)
                + ((bd.Border.Width - textRect.Width) / 2); //центровка на полоске

            var y = bd.VerticalOffset //отступ по вертикали (строки)
                + (bd.Border.Height > FontSize ? ((bd.Border.Height - FontSize) / 2) : 0); //центровка текста по вертикали

            //точные координаты начала текста на Canvas
            Point origin = new Point(x, y);

            var dv = new DrawingVisual();
            var dc = dv.RenderOpen();

            dc.DrawText(ft, origin);

            dc.Close();

            return dv;
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
            if (m_FontSize > 0d && FontFamily != null)
            {
                if (m_drawingFont != null)
                    m_drawingFont.Dispose();

                m_drawingFont = new System.Drawing.Font(new System.Drawing.FontFamily(FontFamily.Source), (float)m_FontSize);
            }
        }

        public override void Dispose()
        {
            if (m_drawingFont != null)
            {
                m_drawingFont.Dispose();
                m_drawingFont = null;
            }

            m_glyphType = null;

            Clear();

            base.Dispose();
        }

        public void Clear()
        {
            foreach (var item in m_bars)
                item.Value.Clear();

            m_bars.Clear();
        }

        /// <summary>
        /// Данные для отрисовки полоски
        /// </summary>
        class BarData
        {
            CanvasTextDrawManager m_manager;
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
            public Rect Border { get; set; }

            public LeasingElementModel BarModel { get; set; }

            /// <summary>
            /// Флаг отрисовки текста для данной полоски
            /// </summary>
            public bool TextDrawed { get; set; }

            public BarData(CanvasTextDrawManager manager)
            {
                m_manager = manager;
            }

            /// <summary>
            /// Удаление линии
            /// </summary>
            public void Clear()
            {
                m_manager = null;
                //m_manager.Canvas.Children.Remove(Border);
                //Border = null;
                BarModel = null;
                TextDrawed = false;
            }
        }

    }
}
