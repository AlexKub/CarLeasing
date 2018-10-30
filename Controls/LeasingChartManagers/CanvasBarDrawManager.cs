using CarLeasingViewer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CarLeasingViewer.Controls.LeasingChartManagers
{
    public delegate void BarDataHandler(CanvasBarDrawManager.BarData bar);
    /// <summary>
    /// Управление отрисовкой полосок занятости авто на Canvas
    /// </summary>
    public class CanvasBarDrawManager : CanvasDrawManager
    {
        double m_halfPenWidth;

        Brush m_brush;
        /// <summary>
        /// Кисть для заливки границы
        /// </summary>
        public Brush BorderBrush
        {
            get { return m_brush; }
            set
            {
                m_brush = value;
                Pen = new Pen(value, AppStyles.GridLineWidth);
                m_halfPenWidth = Pen.Thickness / 2d;
                Pen.Freeze();
            }
        }

        /// <summary>
        /// Основная кисть
        /// </summary>
        public Pen Pen { get; private set; }

        /// <summary>
        /// Кисть для заливки фона полосок
        /// </summary>
        public Brush BackgroundBrush { get; set; }

        /// <summary>
        /// Кисть для заливки заблокированных полосок
        /// </summary>
        public Brush BlockedBarBrush { get; set; }

        double m_rowHeight;
        /// <summary>
        /// Высота строки
        /// </summary>
        public double RowHeight { get { return m_rowHeight; } set { m_rowHeight = value; } }

        /// <summary>
        /// Ширина колонки дня
        /// </summary>
        public double DayColumnWidth { get; set; }

        Dictionary<LeasingElementModel, BarData> m_bars = new Dictionary<LeasingElementModel, BarData>();

        /// <summary>
        /// Отрисованные / просчитанные прямоугольники
        /// </summary>
        public IEnumerable<BarData> Bars { get { return m_bars.Values; } }

        /// <summary>
        /// Данные отрисовки
        /// </summary>
        public IReadOnlyDictionary<LeasingElementModel, BarData> Data { get { return m_bars; } }

        public event BarDataHandler BarAdded;

        /// <summary>
        /// Возвращает данные отрисовки по ссылке на модель
        /// </summary>
        /// <param name="model">Ссылка на отрисовываемую модель</param>
        /// <returns>Возвращает данные отрисовки прямоугольника для модели</returns>
        public BarData this[LeasingElementModel model] { get { if (m_bars.ContainsKey(model)) return m_bars[model]; else return null; } }

        public DrawingVisual DrawBar(LeasingElementModel barModel)
        {
            BarData bd = null;
            DrawingVisual dv = null;

            if (m_bars.ContainsKey(barModel))
                bd = m_bars[barModel];
            else
            {
                bd = new BarData(this);
                bd.Index = barModel.RowIndex;
                bd.VerticalOffset = barModel.RowIndex * RowHeight;
                bd.HorizontalOffset = GetDayOffset(barModel) + GetMonthOffset(barModel);
                bd.Model = barModel;
                m_bars.Add(barModel, bd);
            }

            dv = DrawBorder(bd);

            return dv;
        }

        DrawingVisual DrawBorder(BarData bd)
        {
            var startDay = bd.Model.Leasing.DateStart.Date;

            bool drawGeo = false;
            foreach (var item in Canvas.RowManager[bd.Index].Bars)
            {
                if (item.Model != null)
                    if (item.Model.Leasing.DateEnd.Date == startDay)
                    {
                        drawGeo = true;
                        break;
                    }
            }

            var brush = bd.Model.Leasing.Blocked ? BlockedBarBrush : BackgroundBrush;
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                if (drawGeo)
                    DrawGeometry(dc, bd, brush);
                else
                    DrawRect(dc, bd, brush);

                dc.Close();
            }

            bd.Drawed = true;

            BarAdded?.Invoke(bd);

            return dv;
        }

        void DrawRect(DrawingContext dc, BarData bd, Brush brush)
        {
            Rect rect = new Rect(bd.HorizontalOffset, bd.VerticalOffset, GetWidth(bd.Model), RowHeight);
            bd.Bar = rect;

            //SnapToDevisePixels. See https://www.wpftutorial.net/DrawOnPhysicalDevicePixels.html
            GuidelineSet guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(rect.Left + m_halfPenWidth);
            guidelines.GuidelinesX.Add(rect.Right + m_halfPenWidth);
            guidelines.GuidelinesY.Add(rect.Top + m_halfPenWidth);
            guidelines.GuidelinesY.Add(rect.Bottom + m_halfPenWidth);
            
            dc.PushGuidelineSet(guidelines);
            dc.DrawRectangle(brush, Pen, rect);
            dc.Pop();
        }

        void DrawGeometry(DrawingContext dc, BarData bd, Brush brush)
        {
            PathGeometry g = new PathGeometry();
            PathFigure pf = new PathFigure();
            var start = new Point(bd.HorizontalOffset, bd.VerticalOffset + Canvas.RowHeight);
            var end = new Point(start.X + GetWidth(bd.Model), bd.VerticalOffset);
            pf.StartPoint = start;
            var s = new LineSegment(new Point(end.X, start.Y), true);
            s.Freeze();
            pf.Segments.Add(s);
            s = new LineSegment(new Point(end.X, end.Y), true);
            s.Freeze();
            pf.Segments.Add(s);
            s = new LineSegment(new Point(start.X + Canvas.DayColumnWidth, end.Y), true);
            s.Freeze();
            pf.Segments.Add(new LineSegment(new Point(start.X + Canvas.DayColumnWidth, end.Y), true));
            s = new LineSegment(new Point(start.X, start.Y), true);
            s.Freeze();
            pf.Segments.Add(s);

            pf.Freeze();

            g.Figures.Add(pf);
            g.Freeze();

            bd.Bar = g;
            dc.DrawGeometry(brush, Pen, g);
        }

        public CanvasBarDrawManager(LeasingChart canvas) : base(canvas) { }

        public override void Dispose()
        {
            Clear();

            m_bars = null;

            base.Dispose();
        }

        public void Clear()
        {
            foreach (var item in m_bars.Values)
            {
                item.Clear();
            }

            m_bars.Clear();
        }

        double GetWidth(LeasingElementModel model)
        {
            var dayCount = 0; //прибавляем единичку, так как при сложении/вычитании теряем день

            #region Вычисляем количество дней

            var b = model.Leasing;
            var periodMonthes = Canvas?.LeasingSet?.Monthes.Select(m => m.Month).ToList();

            //если машину взяли/вернули в течении 1 месяца
            if (b.MonthCount == 1)
            {
                dayCount += ((b.DateEnd - b.DateStart).Days + 1);
            }
            //если машина взята в аренду на несколько месяцев
            else
            {
                var firstMonth = periodMonthes.First();

                var startDate = b.DateStart;
                var endDate = b.DateEnd;

                //если съём начался за пределами первого месяца в выбранном периоде
                if (firstMonth > b.DateStart.GetMonth())
                    startDate = firstMonth[1];

                //если съём заканчивается за последним месяцем в выбранном периоде
                if (periodMonthes[periodMonthes.Count - 1] < b.DateEnd.GetMonth())
                    endDate = periodMonthes[periodMonthes.Count - 1].LastDate;

                //получаем месяцы между датами (включительно)
                var monthes = Models.Month.GetMonthes(startDate, endDate);

                //суммируем дни в полученном периоде
                var lastIndex = monthes.Length - 1;
                var endMonth = endDate.GetMonth();
                for (int i = 0; i < monthes.Length; i++)
                {
                    if (i == 0)
                    {
                        if (startDate.Day == 1)
                            if (endMonth == firstMonth)
                                dayCount += endDate.Day;
                            else
                                dayCount += monthes[i].DayCount;
                        else
                            dayCount += (monthes[i].DayCount - startDate.Day + 1);
                    }
                    else if (i == lastIndex)
                    {
                        dayCount += endDate.Day;
                    }
                    else
                        dayCount += monthes[i].DayCount;
                }
            }

            if (dayCount < 0)
            {
                dayCount = 0;
            }

            #endregion

            return (DayColumnWidth * dayCount) + dayCount; //прибавляем количество дней, т.к. ширина границ - 1
        }

        double GetDayOffset(LeasingElementModel model)
        {
            if (model == null)
                return 0d;

            var b = model.Leasing;

            if (b == null)
                return 0d;

            var dayCount = 0;
            var startMonth = b.DateStart.GetMonth();

            var monthes = Canvas?.LeasingSet?.Monthes;

            //если начало в текущем месяце
            if (monthes != null)
            {
                var firstMonth = monthes.ElementAt(0).Month;
                if (firstMonth > startMonth) //если съем начался ранее
                    return 0d;
                else
                {
                    if (firstMonth < startMonth) //если первый месяц выбранного периода начинается раньше
                    {
                        var prevMonth = firstMonth;

                        //перебираем месяцы с лева на право
                        //пока не наткнёмся на начальный месяц съёма авто
                        do
                        {
                            dayCount += prevMonth.DayCount;
                            prevMonth = prevMonth.Next();
                        }
                        while (prevMonth != null && prevMonth != startMonth);
                    }
                }
            }
            //по каким-то причинам не заданы месяцы или дата начала ранее начального месяца
            else if (b.CurrentMonth == null || b.CurrentMonth != startMonth)
                return 0d;

            dayCount += b.DateStart.Day - 1;

            //смещение слева в точках
            return dayCount * DayColumnWidth + (dayCount * 1);
        }

        /// <summary>
        /// Смещение по количеству дней в предидущих месяцах
        /// </summary>
        /// <param name="barModel">Данные текущей полоски месяца</param>
        /// <returns>Возвращает готовой смещение по месяцу или 0</returns>
        double GetMonthOffset(LeasingElementModel barModel)
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

            var monthes = Canvas.LeasingSet.Monthes;

            if (barModel.Monthes != null && barModel.Monthes.Length > 2)
            {
                var firstMonth = barModel.Leasing.DateStart.GetMonth();
                for (int i = 0; i < monthes.Count; i++)
                {
                    if (monthes[i].Month == firstMonth)
                    {
                        var prev = monthes[i].Previous;

                        while (prev != null)
                        {
                            if (prev.Month != null)
                                offset += ((prev.Month.DayCount * DayColumnWidth) + prev.Month.DayCount);

                            prev = prev.Previous;
                        }
                    }
                }
            }

            return offset;
        }

        

        /// <summary>
        /// Данные для отрисовки полоски
        /// </summary>
        [System.Diagnostics.DebuggerDisplay("{DebugerDisplay()}")]
        public class BarData
        {
            CanvasBarDrawManager m_manager;
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
            /// Отрисованный прямоугольник на графике
            /// </summary>
            public Figure Bar { get; set; }

            public LeasingElementModel Model { get; set; }

            /// <summary>
            /// Индекс видимости
            /// </summary>
            public int ZIndex { get; set; } //для случаев, когда в БД пересекаются даты по непонятным причинам

            /// <summary>
            /// Флаг отрисовки линии
            /// </summary>
            public bool Drawed { get; set; }

            /// <summary>
            /// Флаг отрисовки текста для данной полоски
            /// </summary>
            public bool TextDrawed { get; set; }

            public BarData(CanvasBarDrawManager manager)
            {
                m_manager = manager;
            }
            public BarData(LeasingElementModel model)
            {
                Model = model;
            }

            /// <summary>
            /// Удаление линии
            /// </summary>
            public void Clear()
            {
                if (Drawed)
                {
                    //m_manager.Canvas.Children.Remove(Border);
                    Drawed = false;
                    Model = null;
                }
            }

            string DebugerDisplay()
            {
                return Index.ToString() + " | " + (Model == null ? "NO_MODEL" : Model.CarName);
            }

            /// <summary>
            /// Фигура на графике
            /// </summary>
            [System.Diagnostics.DebuggerDisplay("{DebugerDisplay()}")]
            public class Figure
            {
                readonly FigureType m_type;

                /// <summary>
                /// Прямоугольник
                /// </summary>
                public Rect Rectangle { get; private set; }
                /// <summary>
                /// Многоугольник
                /// </summary>
                public Geometry Geometry { get; private set; }
                /// <summary>
                /// Ширина фигуры
                /// </summary>
                public double Width { get; private set; }
                /// <summary>
                /// Тип фигуры
                /// </summary>
                public FigureType Type { get { return m_type; } }

                /// <summary>
                /// Проверка пересечения фигур
                /// </summary>
                /// <param name="f">Другая фигура</param>
                /// <returns>Возвращает true, если описывающие прямоугольники пересекаются</returns>
                public bool IntersectsWith(Figure f)
                {
                    switch (Type)
                    {
                        case FigureType.Rect:
                            return f.Type == FigureType.Rect ? Rectangle.IntersectsWith(f.Rectangle) : Rectangle.IntersectsWith(f.Geometry.Bounds);
                        case FigureType.Geometry:
                            return f.Type == FigureType.Rect ? Geometry.Bounds.IntersectsWith(f.Rectangle) : Geometry.Bounds.IntersectsWith(f.Geometry.Bounds);
                        default:
                            return false;
                    }
                }

                public bool Contains(Point p)
                {
                    switch (Type)
                    {
                        case FigureType.Rect:
                            return Rectangle.Contains(p);
                        case FigureType.Geometry:
                            return Geometry.FillContains(p);
                        default:
                            return false;
                    }
                }

                public Figure(Rect r)
                {
                    Rectangle = r;
                    m_type = FigureType.Rect;
                    Width = r.Width;
                }
                public Figure(Geometry g)
                {
                    Geometry = g;
                    m_type = FigureType.Geometry;
                    Width = Geometry.Bounds.Width;
                }

                public static implicit operator Rect(Figure f)
                {
                    if (f == null)
                        return new Rect();

                    return f.Rectangle;
                }
                public static implicit operator Geometry(Figure g) => g?.Geometry;

                public static implicit operator Figure(Rect r) => new Figure(r);
                public static implicit operator Figure(Geometry g) => new Figure(g);

                string DebugerDisplay()
                {
                    return Type.ToString() + " | " + Width.ToString();
                }

                /// <summary>
                /// Типы фигур
                /// </summary>
                public enum FigureType
                {
                    /// <summary>
                    /// Прямоугольник
                    /// </summary>
                    Rect,
                    /// <summary>
                    /// Многоугольник
                    /// </summary>
                    Geometry
                }
            }
        }


    }
}
