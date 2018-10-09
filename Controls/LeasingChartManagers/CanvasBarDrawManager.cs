using CarLeasingViewer.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CarLeasingViewer.Controls.LeasingChartManagers
{
    /// <summary>
    /// Управление отрисовкой полосок занятости авто на Canvas
    /// </summary>
    public class CanvasBarDrawManager : CanvasDrawManager
    {
        /// <summary>
        /// Кисть для заливки границы
        /// </summary>
        public Brush BorderBrush { get; set; }

        /// <summary>
        /// Кисть для заливки фона полосок
        /// </summary>
        public Brush BackgroundBrush { get; set; }

        public double RowHeight { get; set; }

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

                //var lineNumber = barModel.RowIndex + 1;
                bd.VerticalOffset = barModel.RowIndex * RowHeight;
                bd.HorizontalOffset = barModel.DayOffset + GetMonthOffset(barModel);
                bd.BarModel = barModel;
                m_bars.Add(barModel, bd);
            }

            dv = DrawBorder(bd);
            //bd.Border = b;

            return dv;
        }

        DrawingVisual DrawBorder(BarData bd)
        {
            Pen pen = new Pen(BorderBrush, 1);
            Rect rect = new Rect(bd.HorizontalOffset, bd.VerticalOffset, bd.BarModel.Width, RowHeight);
            bd.Border = rect;

            //SnapToDevisePixels. See https://www.wpftutorial.net/DrawOnPhysicalDevicePixels.html
            double halfPenWidth = pen.Thickness / 2;
            GuidelineSet guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(rect.Left + halfPenWidth);
            guidelines.GuidelinesX.Add(rect.Right + halfPenWidth);
            guidelines.GuidelinesY.Add(rect.Top + halfPenWidth);
            guidelines.GuidelinesY.Add(rect.Bottom + halfPenWidth);

            var dv = new DrawingVisual();
            var dc = dv.RenderOpen();

            dc.PushGuidelineSet(guidelines);
            dc.DrawRectangle(BackgroundBrush, pen, rect);
            dc.Pop();

            dc.Close();

            bd.Drawed = true;

            return dv;
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

            if (barModel.Monthes != null && barModel.Monthes.Length > 0)
            {
                if (barModel.Monthes[0].Previous != null)
                {
                    var prev = barModel.Monthes[0].Previous;

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
        /// Данные для отрисовки полоски
        /// </summary>
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
            /// Отрисованная граница
            /// </summary>
            public Rect Border { get; set; }

            public LeasingElementModel BarModel { get; set; }

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

            /// <summary>
            /// Удаление линии
            /// </summary>
            public void Clear()
            {
                if (Drawed)
                {
                    //m_manager.Canvas.Children.Remove(Border);
                    Drawed = false;
                    BarModel = null;
                }
            }
        }
    }
}
