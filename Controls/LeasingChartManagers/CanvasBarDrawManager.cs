using CarLeasingViewer.Models;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace CarLeasingViewer
{
    partial class LeasingChart
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

            public void DrawBar(LeasingElementModel barModel)
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





            public CanvasBarDrawManager(Canvas canvas) : base(canvas)
            {

            }

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
            /// Данные для отрисовки полоски
            /// </summary>
            class BarData
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
                        m_manager.Canvas.Children.Remove(Border);
                        Border = null;
                        BarModel = null;
                    }
                }
            }
        }
    }
}
