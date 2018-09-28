using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CarLeasingViewer
{
    partial class LeasingChart {

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

            public CanvasTextDrawManager(Canvas canvas) : base(canvas)
            {
                //цвет текста по умолчанию
                TextBrush = Brushes.Black;
                TextBrush.Freeze();
            }

            int drawIndex = 0;
            public void DrawText(DrawingContext dc)
            {
                drawIndex++;
                //var sb = new SBWriter();
                //
                //ObjectDumper.Write(dc, 0, sb);
                //
                //File.WriteAllText("od_" + drawIndex.ToString() + ".txt", sb.Text);

                if (drawIndex == 4)
                {
                    //File.WriteAllText("Json_" + drawIndex.ToString() + ".txt", JObject.FromObject(dc).ToString());

                    //drawIndex++;

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
                //using (var sr = File.AppendText("SomeFile.txt"))
                //    sr.Write(drawIndex.ToString());
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
                string text = bd?.BarModel?.Leasing?.Title + "_" + bd.BarModel.Leasing.CarName ?? "NO TITLE";
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

                base.Dispose();
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
                public Border Border { get; set; }

                public LeasingElementModel BarModel { get; set; }

                /// <summary>
                /// Флаг отрисовки линии
                /// </summary>
                public bool Drawed { get { return Border != null; } }

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
}
