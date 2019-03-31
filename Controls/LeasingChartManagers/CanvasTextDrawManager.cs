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
                m_halfColumnWidth = value / 2;
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
            if (border != null)
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
                bd = new BarData(this) { Index = model.RowIndex, Model = model };
                m_bars.Add(model, bd);
                SetOffset(bd);
            }

            //если ещё не проставлены смещения
            if (bd.Border == null)
                SetOffset(bd);

            //нет смысла в отрисовке текста без отрисованного прямоугольника
            if (bd.BorderDrawed)
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
            string text = bd?.Model?.Text ?? "NO TITLE";

            //обрезаем ООО для компаний, т.к. информация бессмысленная
            //в tooltip'e и так видно абревиатуру ЮЛ, а при аренде на пару дней видно только ООО
            text = text.Replace("ООО ", "");
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

            //получаем размеры пустой области для текста на полоске
            //вычитая несколько пикселей из ширины полоски для отступа текста от краёв
            var emptySpace = bd.Border.Type == Figure.FigureType.Rect
                ? bd.Border.Width - 4d //для прямоугольников вычитаем по 2 пикселя с каждой стороны
                : bd.Border.Width - 4d - DayColumnWidth; //для усечённых прямоугольников вычитаем ещё половину ширины одной колонки

            //флаг аренды на часть дня. 
            //Будет использован дважды, поэтому проверяем один раз тут
            bool halfDayLeasing = bd.Model.VisibleDaysCount == 1 && bd.Border.Type == Figure.FigureType.Geometry;
            if (emptySpace < ft.Width)
            {
                var cuttedText = string.Empty;

                //если аренда на часть дня - места совсем нет
                //просто добаявляем точки
                if (bd.Model.VisibleDaysCount == 1 && bd.Border.Type == Figure.FigureType.Geometry)
                    cuttedText = "...";

                else
                {
                    var cutChars = 3 * bd.Model.VisibleDaysCount;

                    //отрезаем пару букв справа для усечённых
                    //т.к. для них текст будет немного смещён вправо
                    if (bd.Border.Type == Figure.FigureType.Geometry)
                    {
                        if (cutChars > 3)
                            cutChars = cutChars - 2;
                    }
                    if (text.Length <= cutChars)
                        cuttedText = text;
                    else
                        cuttedText = text.Substring(0, cutChars);
                }

                ft = new FormattedText(cuttedText, culture, ft.FlowDirection, m_Typeface, fontSize, TextBrush);
            }

            //отступ по горизонтали (дни)
            var x = GetHorizontalOffset(bd, ft);

            var y = bd.VerticalOffset //отступ по вертикали (строки)
                + (Canvas.RowHeight > FontSize ? ((Canvas.RowHeight - FontSize) / 2d) : 0d); //центровка текста по вертикали

            //точные координаты начала текста на Canvas
            Point origin = new Point(x, y);

            var dv = new DrawingVisual();
            var dc = dv.RenderOpen();

            dc.DrawText(ft, origin);

            dc.Close();

            return dv;
        }

        double GetHorizontalOffset(BarData bd, FormattedText ft)
        {
            var textRect = new Size(ft.Width, ft.Height); //System.Windows.Forms.TextRenderer.MeasureText(text, m_drawingFont); 

            if (bd.Border.Type == Figure.FigureType.Rect)
                //для обычных прямоугольников располагаем по центру
                return bd.HorizontalOffset + ((bd.Border.Width - textRect.Width) / 2d);

            //для усечённых прямоугольников
            else
            {
                //для обрезанных слева - двигаем вправо
                return bd.HorizontalOffset + GetGeometryOffset(bd, textRect);
            }
        }
        /// <summary>
        /// Получение смещения текста для усеченных прямоугольников
        /// </summary>
        /// <param name="bd">Данные отрисованно полоски</param>
        /// <param name="textRect">Геометрия отрисовываемого текста</param>
        /// <returns></returns>
        double GetGeometryOffset(BarData bd, Size textRect)
        {
            var midle = ((bd.Border.Width - textRect.Width) / 2d);
            var offset = ((bd.Model.VisibleDaysCount == 1 && bd.Border.Type == Figure.FigureType.Geometry)
                        ? 3d //смещаем чуть-чуть только, т.к. места и так нет. Тут должно быть троеточие
                        : m_halfColumnWidth); //если место есть, смещаем немного правее, чтобы буквы не вылезали

            return bd.Border.PathType == CanvasBarDrawManager.DrawPathType.Geometry_R
                ? midle - offset
                : midle + offset;
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
