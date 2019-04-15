﻿using CarLeasingViewer.Interfaces;
using CarLeasingViewer.Models;
using System;
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
        /// <summary>
        /// Ширина колонки + ширина линии сетки
        /// </summary>
        double m_OffsetColumnWidth;
        /// <summary>
        /// Смещение по строке (высота строки + ширина полоски)
        /// </summary>
        double m_rowOffset;
        Pen m_currentPen;
        Brush m_currentBrush;

        Pen m_MaintenancePen;
        Pen m_StornoPen;
        Brush m_StornoBrush;

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
                LeasingPen = new Pen(value, AppStyles.GridLineWidth);
                m_halfPenWidth = LeasingPen.Thickness / 2d;
                LeasingPen.Freeze();
            }
        }

        /// <summary>
        /// Основная кисть
        /// </summary>
        public Pen LeasingPen { get; private set; }

        Brush m_LeasingBrush;
        /// <summary>
        /// Кисть для заливки фона полосок
        /// </summary>
        public Brush LeasingBrush
        {
            get { return m_LeasingBrush; }
            set
            {
                m_LeasingBrush = value;
                m_LeasingBrush.Freeze();

                if (value != null)
                {
                    m_StornoBrush = GetStornoBrush((value as SolidColorBrush).Color);
                    m_StornoPen = new Pen(m_StornoBrush, 3d);
                    m_StornoPen.Freeze();
                }
            }
        }

        /// <summary>
        /// Кисть для заливки заблокированных полосок
        /// </summary>
        public Brush BlockedBarBrush { get; set; }

        Brush m_MaintenanceBrush;
        /// <summary>
        /// Кисть для заливки находящихся в ремонте
        /// </summary>
        public Brush MaintenanceBrush
        {
            get
            {
                return m_MaintenanceBrush;
            }
            set
            {
                m_MaintenanceBrush = value;

                //простановка  отрисовки зеброй
                m_MaintenancePen = new Pen(value, 3d);
                m_MaintenancePen.Freeze();
            }
        }

        double m_rowHeight;
        /// <summary>
        /// Высота строки
        /// </summary>
        public double RowHeight
        {
            get { return m_rowHeight; }
            set
            {
                m_rowHeight = value;
                m_rowOffset = value + AppStyles.GridLineWidth;
            }
        }

        double m_DayColumnWidth;
        /// <summary>
        /// Ширина колонки дня
        /// </summary>
        public double DayColumnWidth
        {
            get { return m_DayColumnWidth; }
            set
            {
                m_DayColumnWidth = value;
                m_OffsetColumnWidth = value + AppStyles.GridLineWidth;
            }
        }

        Dictionary<IDrawableBar, BarData> m_bars = new Dictionary<IDrawableBar, BarData>();

        /// <summary>
        /// Отрисованные / просчитанные прямоугольники
        /// </summary>
        public IEnumerable<BarData> Bars { get { return m_bars.Values; } }

        /// <summary>
        /// Данные отрисовки
        /// </summary>
        public IReadOnlyDictionary<IDrawableBar, BarData> Data { get { return m_bars; } }

        public event BarDataHandler BarAdded;

        /// <summary>
        /// Возвращает данные отрисовки по ссылке на модель
        /// </summary>
        /// <param name="model">Ссылка на отрисовываемую модель</param>
        /// <returns>Возвращает данные отрисовки прямоугольника для модели</returns>
        public BarData this[IDrawableBar model] { get { if (m_bars.ContainsKey(model)) return m_bars[model]; else return null; } }

        public DrawingVisual DrawBar(IDrawableBar barModel)
        {
            BarData bd = null;
            DrawingVisual dv = null;

            if (m_bars.ContainsKey(barModel))
                bd = m_bars[barModel];
            else
            {
                bd = new BarData(this);
                bd.Index = barModel.RowIndex;
                bd.VerticalOffset = barModel.RowIndex * m_rowHeight;
                bd.HorizontalOffset = GetDayOffset(barModel);
                bd.Model = barModel;
                m_bars.Add(barModel, bd);
            }

            dv = DrawBorder(bd);

            return dv;
        }

        DrawingVisual DrawBorder(BarData bd)
        {
            DrawingVisual dv = null;
            
            if (bd.Model != null && bd.Model.Visible)
            {
                //тип отрисовываемой фигуры
                var pathType = ChoosePathType(bd);
                //инструменты отрисовки
                SetDrawTools(bd.Model.BarType);

                dv = new DrawingVisual();
                using (var dc = dv.RenderOpen())
                {
                    if ((pathType & DrawPathType.Rectangle) > 0)
                        DrawRect(dc, bd);
                    else if ((pathType & DrawPathType.Image) > 0)
                        DrawImage(dc, bd);
                    else
                        DrawGeometry(dc, bd, pathType);

                    bd.Bar.PathType = pathType;
                    dc.Close();
                }

                bd.Drawed = true;
            }

            BarAdded?.Invoke(bd);

            return dv;
        }

        DrawPathType ChoosePathType(BarData bd)
        {
            DrawPathType pathType = DrawPathType.UnKnown;

            switch (bd.Model.BarType)
            {
                case ChartBarType.Storno:
                case ChartBarType.Maintenance:
                case ChartBarType.Leasing:
                    var period = bd.Model.Period;
                    if (period.DateStart.Hour >= 12)
                    {
                        if (period.DayIndexStart >= (bd.Model.Set as IPeriod).DayIndexStart)
                            pathType = DrawPathType.Geometry_L;
                    }
                    if (period.DateEnd.Hour <= 12)
                    {
                        pathType |= DrawPathType.Geometry_R;
                    }

                    //если левого скоса ещё нет
                    if ((pathType & DrawPathType.Geometry_L) == 0)
                    {
                        var startDay = period.DateStart.Date;

                        //если у какой-то панели дата окончания совпадает
                        //с датой начала у этой панели
                        var hasPredecessor = Canvas.RowManager[bd.Index].Bars
                            .Any(b => b.Visible 
                                && b.Bar.PathType != DrawPathType.Image 
                                && b.Model.Period.DateEnd.Date == startDay);

                        if(hasPredecessor)
                            //рисуем скос у накладывающихся друг на друга сроков аренды
                            pathType |= DrawPathType.Geometry_L;
                    }

                    //если скосов нет - делаем прямоугольник
                    if (pathType == DrawPathType.UnKnown)
                        pathType = DrawPathType.Rectangle;

                    break;
                case ChartBarType.Insurance:
                    pathType = DrawPathType.Image;
                    break;
                default:
                    break;
            }

            return pathType;
        }

        /// <summary>
        /// Простановка настроек отрисовки
        /// </summary>
        /// <param name="type">Тип панели</param>
        void SetDrawTools(ChartBarType type)
        {
            switch (type)
            {
                //отрисовка панелек Занятости
                case ChartBarType.Leasing:
                    m_currentPen = LeasingPen;
                    m_currentBrush = LeasingBrush;
                    break;
                //отрисовка панелек Ремонта
                case ChartBarType.Maintenance:
                    m_currentPen = LeasingPen;
                    m_currentBrush = m_MaintenanceBrush;
                    break;
                case ChartBarType.Insurance:
                    break;
                case ChartBarType.Storno:
                    m_currentPen = LeasingPen;
                    m_currentBrush = m_StornoBrush;
                    break;
                default:
                    throw new NotImplementedException($"Отрисовка для типа модели '{type.ToString()}' не реализована");
            }
        }

        void DrawRect(DrawingContext dc, BarData bd)
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

            dc.DrawRectangle(m_currentBrush, m_currentPen, rect);
            dc.Pop();
        }

        void DrawGeometry(DrawingContext dc, BarData bd, DrawPathType path)
        {
            PathGeometry g = new PathGeometry();
            PathFigure pf = new PathFigure();

            //левая нижняя точка
            var start = new Point(bd.HorizontalOffset, bd.VerticalOffset + RowHeight);
            //правая верхняя точка
            var end = new Point(start.X + GetWidth(bd.Model), bd.VerticalOffset);

            pf.StartPoint = start;
            //правая нижняя
            var s = (path & DrawPathType.Geometry_R) > 0
                ? new LineSegment(new Point(end.X - Canvas.DayColumnWidth, start.Y), true)
                : new LineSegment(new Point(end.X, start.Y), true);
            s.Freeze();
            pf.Segments.Add(s);
            //правая верхняя
            s = new LineSegment(end, true);
            s.Freeze();
            pf.Segments.Add(s);
            //левая верхняя
            s = (path & DrawPathType.Geometry_L) > 0
                ? new LineSegment(new Point(start.X + Canvas.DayColumnWidth, end.Y), true)
                : new LineSegment(new Point(start.X, end.Y), true);
            s.Freeze();
            pf.Segments.Add(s);
            //левая нижняя
            s = new LineSegment(start, true);
            s.Freeze();
            pf.Segments.Add(s);

            pf.Freeze();

            g.Figures.Add(pf);
            g.Freeze();

            //SnapToDevisePixels. See https://www.wpftutorial.net/DrawOnPhysicalDevicePixels.html
            GuidelineSet guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(start.X + m_halfPenWidth);
            guidelines.GuidelinesX.Add(end.X + m_halfPenWidth);
            guidelines.GuidelinesY.Add(start.Y + m_halfPenWidth);
            guidelines.GuidelinesY.Add(end.Y + m_halfPenWidth);

            bd.Bar = g;
            dc.PushGuidelineSet(guidelines);
            dc.DrawGeometry(m_currentBrush, m_currentPen, g);
        }

        void DrawImage(DrawingContext dc, BarData bd)
        {
            var image = bd.Model as ImageBarModel;

            if (image == null || image.Bitmap == null)
                return;

            //левая нижняя точка
            var start = new Point(bd.HorizontalOffset, bd.VerticalOffset + RowHeight);
            //правая верхняя точка
            var end = new Point(start.X + GetWidth(bd.Model), bd.VerticalOffset);
            var rect = new Rect(start, end);

            //SnapToDevisePixels. See https://www.wpftutorial.net/DrawOnPhysicalDevicePixels.html
            GuidelineSet guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(start.X + m_halfPenWidth);
            guidelines.GuidelinesX.Add(end.X + m_halfPenWidth);
            guidelines.GuidelinesY.Add(start.Y + m_halfPenWidth);
            guidelines.GuidelinesY.Add(end.Y + m_halfPenWidth);

            bd.Bar = rect;
            dc.PushGuidelineSet(guidelines);
            dc.DrawImage(image.Bitmap, rect);
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

        double GetWidth(IDrawableBar model)
        {
            var dayCount = 0; //прибавляем единичку, так как при сложении/вычитании теряем день

            #region Вычисляем количество дней

            var b = model.Period;
            var periodMonthes = Canvas?.LeasingSet?.Monthes.Select(m => m.Month).ToList();

            //если машину взяли/вернули в течении 1 месяца
            if (b.MonthCount == 1)
            {
                dayCount += ((b.DateEnd.Date - b.DateStart.Date).Days + 1);
            }
            //если машина взята в аренду на несколько месяцев
            else
            {
                var firstMonth = periodMonthes.First();

                var startDate = b.DateStart.Date;
                var endDate = b.DateEnd.Date;

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

            return DayColumnWidth * dayCount;
        }

        /// <summary>
        /// Полосатая кисть
        /// </summary>
        /// <returns>Возвращает кисть с выбранным цветом</returns>
        Brush GetStornoBrush(Color color)
        {
            return new LinearGradientBrush()
            {
                MappingMode = BrushMappingMode.Absolute,
                StartPoint = new Point(4, 4),
                EndPoint = new Point(0, 0),
                SpreadMethod = GradientSpreadMethod.Repeat,

                GradientStops = new GradientStopCollection()
                {
                      new GradientStop() { Offset = 0d, Color = color }
                    , new GradientStop() { Offset = 0.6d, Color = color }
                    , new GradientStop() { Offset = 0.6d, Color = Colors.White }
                    , new GradientStop() { Offset = 1d, Color = Colors.White }
                }
            };
        }

        /// <summary>
        /// Расчёт смещения полоски от левого края графика
        /// </summary>
        /// <param name="model">Отрисовываемая модель</param>
        /// <returns>Возвращает смещение полоски от левого края графика</returns>
        double GetDayOffset(IDrawableBar model)
        {
            /*
             * расчёт смещения полоски от левого края графика
             * 
             */

            if (model == null)
                return 0d;

            var b = model.Period;//model.Leasing;

            if (b == null)
                return 0d;

            var dayCount = 0;
            var modelStartMonth = b.DateStart.GetMonth();

            var monthes = Canvas.LeasingSet.Monthes;
            var viewFirstMonth = monthes.FirstOrDefault()?.Month;

            if (viewFirstMonth == null)
            {
                App.Loger.Log("Не удалось получить первого месяца набора при отрисовке графика");
            }
            //для тех, кто началася в прошлом периоде
            //никакого отступа
            else if (modelStartMonth.Index < viewFirstMonth.Index)
                return 0;
            //для моделей, чья дата начала позже текущего первого месяца
            else if (modelStartMonth.Index > viewFirstMonth.Index)
            {
                Month curMonth = viewFirstMonth;
                int offset = 0;
                //перебираем месяца, 
                //считая суммарное количество дней
                do
                {
                    offset += curMonth.DayCount;
                    curMonth = curMonth.Next();
                }
                while (curMonth != modelStartMonth);

                //смещаемся на графике в соответствующий месяц
                //по количеству дней
                dayCount += offset;
            }

            //добавляем количество дней в текущем месяце
            dayCount += b.DateStart.Day - 1;

            //смещение слева в точках
            return dayCount * m_DayColumnWidth;
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
            /// <summary>
            /// Модель
            /// </summary>
            public IDrawableBar Model { get; set; }
            /// <summary>
            /// Индекс видимости
            /// </summary>
            public int ZIndex { get; set; } //для случаев, когда в БД пересекаются даты по непонятным причинам
            /// <summary>
            /// Флаг отрисовки линии
            /// </summary>
            public bool Drawed { get; set; }
            /// <summary>
            /// Видимость
            /// </summary>
            public bool Visible => Bar != null;

            /// <summary>
            /// Флаг отрисовки текста для данной полоски
            /// </summary>
            public bool TextDrawed { get; set; }

            public BarData(CanvasBarDrawManager manager)
            {
                m_manager = manager;
            }
            public BarData(IDrawableBar model)
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
                var titled = Model as ITitledBar;
                return Index.ToString() + " | " +
                    (titled == null
                        ? Model == null
                            ? "NO_MODEL"
                            : Model.GetType().Name
                        : titled.Title.LogValue());
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
                /// Тип кривой, описывающей фигуру
                /// </summary>
                public DrawPathType PathType { get; set; }

                /// <summary>
                /// Проверка пересечения фигур
                /// </summary>
                /// <param name="f">Другая фигура</param>
                /// <returns>Возвращает true, если описывающие прямоугольники пересекаются</returns>
                public bool IntersectsWith(Figure f)
                {
                    if (f == null)
                        return false;

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

        /// <summary>
        /// Типы отрисовываемых кривых
        /// </summary>
        [Flags]
        public enum DrawPathType
        {
            /// <summary>
            /// Не определён (по умолчанию)
            /// </summary>
            UnKnown = 0,
            /// <summary>
            /// Прямоугольник
            /// </summary>
            Rectangle = 1,
            /// <summary>
            /// Скос слева
            /// </summary>
            Geometry_L = 2,
            /// <summary>
            /// Скос справа
            /// </summary>
            Geometry_R = 4,
            /// <summary>
            /// Картинка
            /// </summary>
            Image = 8
        }

    }
}
