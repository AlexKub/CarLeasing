using CarLeasingViewer.Interfaces;
using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using static CarLeasingViewer.Controls.LeasingChartManagers.CanvasBarDrawManager.BarData;

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
        /// Половина ширины колонки для расчёта
        /// </summary>
        double m_halfColumnWidth;

        /// <summary>
        /// Шрифт для расчёта размеров текста. Кешируем один раз, чтобы не генерировать кучу экземпляров.
        /// </summary>
        System.Drawing.Font m_drawingFont;

        Dictionary<ITitledBar, BarData> m_bars = new Dictionary<ITitledBar, BarData>();

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

        private double m_DayColumnWidth;

        public double DayColumnWidth
        {
            get { return m_DayColumnWidth; }
            set
            {
                m_DayColumnWidth = value;
                m_halfColumnWidth = value / 3;
            }
        }


        /// <summary>
        /// Загрузка набора для отрисовки
        /// </summary>
        /// <param name="data">Данные для отрисовки</param>
        public void Load(IEnumerable<LeasingBarModel> data)
        {
            foreach (var item in data)
            {
                //создаём модель данных для отрисовки текста
                var bd = new BarData(this) { Index = item.RowIndex, Model = item };

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
            var border = Canvas.BarManager[bd.Model];
            if (border != null && border.Visible)
            {
                bd.Border = border.Bar;
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

        public DrawingVisual DrawText(ITitledBar model)
        {
            BarData bd = null;
            DrawingVisual dv = null;

            if (m_bars.ContainsKey(model))
                bd = m_bars[model];
            else
            {
                bd = new BarData(this) { Index = model.RowIndex, Model = model };
                m_bars.Add(model, bd);
                SetOffset(bd);
            }

            //если ещё не проставлены смещения
            if (bd.Border == null)
                SetOffset(bd);

            //нет смысла в отрисовке текста без отрисованного прямоугольника
            if (bd.BorderDrawed && bd.Model.Visible)
            {
                dv = DrawText(bd);
                bd.TextDrawed = true;
            }

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
            string text = string.Empty; //bd?.Model?.Title ?? "NO TITLE";
            double fontSize = m_FontSize;
            int visibleDaysCount = GetVisibleDaysCount(bd);
            //для обрезанных нужны доп. подсчёты
            bool cuttedBar = bd.Border.Type == Figure.FigureType.Geometry;

            //флаг аренды на часть дня. 
            bool halfDayLeasing = visibleDaysCount == 1 && cuttedBar;
            
            if (halfDayLeasing)
                text = "...";
            else
            {
                text = bd?.Model?.Title ?? "NO TITLE";

                //обрезаем ООО для компаний, т.к. информация бессмысленная
                //в tooltip'e и так видно абревиатуру ЮЛ, а при аренде на пару дней видно только ООО
                text = text.Replace("ООО ", "");
            }

            //рамка текста (ширина/высота)
            //расчитываем ширину / высоту перед отрисовкой для центровки текста
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            FormattedText ft = new FormattedText(text, culture, FlowDirection.LeftToRight, m_Typeface, fontSize, TextBrush);

            //получаем размеры пустой области для текста на полоске
            //вычитая несколько пикселей из ширины полоски для отступа текста от краёв
            var emptySpace = visibleDaysCount * m_DayColumnWidth - 4d;
            //? bd.Border.Width - 4d //для прямоугольников вычитаем по 2 пикселя с каждой стороны
            //: bd.Border.Width - 4d; //для усечённых прямоугольников вычитаем ещё половину ширины одной колонки

            if (cuttedBar)
                emptySpace -= m_halfColumnWidth;

            if (emptySpace < ft.Width)
            {
                var cuttedText = string.Empty;

                //если аренда на часть дня - места совсем нет
                //просто добавляем точки
                if (halfDayLeasing)
                    cuttedText = "...";
                else
                {
                    var cutChars = 3 * (cuttedBar ? visibleDaysCount - 1 : visibleDaysCount);

                    
                    if (text.Length <= cutChars)
                        cuttedText = text;
                    else
                    {
                        //отрезаем пару букв справа для усечённых
                        //т.к. для них текст будет немного смещён вправо

                        if (cuttedBar)
                        {
                            if (cutChars > 3)
                                cutChars = cutChars - 2;
                        }

                        cuttedText = text.Substring(0, cutChars);
                    }
                }

                ft = new FormattedText(cuttedText, culture, ft.FlowDirection, m_Typeface, fontSize, TextBrush);
            }

            //отступ по горизонтали
            var x = bd.HorizontalOffset + ((bd.Border.Width - ft.Width) / 2d);
            if(cuttedBar)
            {
                x += GetGeometryOffset(bd, visibleDaysCount == 1);
            }

            //отступ по вертикали
            var y = bd.VerticalOffset 
                + (Canvas.RowHeight > FontSize ? ((Canvas.RowHeight - FontSize) / 2d) : 0d); //центровка текста по вертикали

            //точные координаты начала текста на Canvas
            Point origin = new Point(x, y);

            var dv = new DrawingVisual();
            var dc = dv.RenderOpen();

            dc.DrawText(ft, origin);

            dc.Close();

            return dv;
        }

        int GetVisibleDaysCount(BarData bd)
        {
            if (!bd.Model.Visible)
                return 0;

            return bd.Model.Set.CrossDaysCount(bd.Model.Period);
        }

        double GetTextWidth(string text)
        {
            ushort[] glyphIndexes = new ushort[text.Length];
            double[] advanceWidths = new double[text.Length];

            double totalWidth = 0;

            for (int n = 0; n < text.Length; n++)
            {
                ushort glyphIndex = m_glyphType.CharacterToGlyphMap[text[n]];
                glyphIndexes[n] = glyphIndex;

                double width = m_glyphType.AdvanceWidths[glyphIndex] * m_FontSize;
                advanceWidths[n] = width;

                totalWidth += width;
            }

            return totalWidth;
        }

        /// <summary>
        /// Получение смещения текста для усеченных прямоугольников
        /// </summary>
        /// <param name="bd">Данные отрисованно полоски</param>
        /// <param name="textRect">Геометрия отрисовываемого текста</param>
        /// <returns></returns>
        double GetGeometryOffset(BarData bd, bool oneDay)
        {
            var offset = 0d;
            var pathType = bd.Border.PathType;
            //если усечение одно: только с лева или справа
            if (pathType != (CanvasBarDrawManager.DrawPathType.Geometry_L | CanvasBarDrawManager.DrawPathType.Geometry_R))
            {
                /*
                 * 
                 * добавляем небольшое смещение
                 * т.к. из-за скоса текст может вылезать
                 * когда ширина его близка к ширине фигуры
                 * 
                 */

                //для периодов в 1 день места совсем нет
                //по этому для них добавляем пару пикселей
                offset = oneDay ? 2d : m_halfColumnWidth;

                //если скос справа - вычитаем смещение (инверитруем)
                //сдвигая текст чуть левее
                if ((pathType & CanvasBarDrawManager.DrawPathType.Geometry_R) > 0)
                    offset = offset * -1d;
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
            public Figure Border { get; set; }

            public ITitledBar Model { get; set; }

            /// <summary>
            /// Флаг отрисовки текста для данной полоски
            /// </summary>
            public bool TextDrawed { get; set; }

            /// <summary>
            /// Проверка наличия отрисовканного прямоугольника
            /// </summary>
            public bool BorderDrawed { get { return Border != null; } }

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
                Model = null;
                TextDrawed = false;
            }
        }

    }
}
