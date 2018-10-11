using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CarLeasingViewer.Controls.LeasingChartManagers
{
    /// <summary>
    /// Управление отображением Tooltip'a
    /// </summary>
    public class TooltipManager : IDisposable
    {
        LeasingChart m_chart;
        DrawingVisual m_tooltipVisual;
        Grid m_tooltip;
        CanvasBarDrawManager.BarData m_TooltipedRect;

        /// <summary>
        /// Видимая область для рисования
        /// </summary>
        public VisibleArea VisibleArea { get; set; }

        public TooltipManager(LeasingChart chart)
        {
            m_chart = chart;
            VisibleArea = new VisibleArea();
        }

        /// <summary>
        /// Обработка Tooltip'a
        /// </summary>
        /// <param name="point">Текущая точка курсора мыши</param>
        public void HandleTooltip(Point point)
        {
            //проверка позиционироввания курсора над ранее обработанным элементом
            if (m_TooltipedRect != null)
            {
                //если мышь всё ещё над тем же элементом - ничего не делаем
                if (m_TooltipedRect.Border.Contains(point))
                {
                    //двигаем tooltip вместе с мышкой, чтобы не закрывал полоску
                    MoveTooltip(point);
                }
                else //если мышь ушла с элемента
                {
                    //скрываем подсказку
                    HideTooltip();
                }
            }

            //поиск элемента, над которым сейчас находится мышь
            //получаем строку, к которой принадлежит точка
            var row = m_chart.RowManager.GetRowByPoint(point);

            if (row != null)
            {
                //для случая, когда несколько полосок друг на другая наслаиваются
                //ищем ту, что видна пользователю - с наибольшим ZIndex
                CanvasBarDrawManager.BarData maxZ = null;
                foreach (var b in row.Bars)
                {
                    if (b.Border.Contains(point))
                        if (maxZ == null)
                            maxZ = b;
                        else
                            maxZ = maxZ.ZIndex > b.ZIndex ? maxZ : b;
                }

                //если пересечение с точкой найдено
                if(maxZ != null)
                {
                    HideTooltip();

                    ShowTooltip(maxZ, point);
                }
            }
            //foreach (var kvp in m_chart.BarManager.Data)
            //{
            //    bar = kvp.Value;
            //    if (bar.VerticalOffset <= point.Y)
            //    {
            //        if (bar.Border.Contains(point))
            //        {
            //            HideTooltip();
            //
            //            ShowTooltip(bar, point);
            //            return;
            //        }
            //    }
            //}
        }

        void MoveTooltip(Point p)
        {
            //двигаем tooltip при движении мыши
            if (m_tooltip == null)
                return;

            var oldTooltip = m_tooltip;
            var oldRect = m_TooltipedRect;
            if (p.X + oldTooltip.ActualWidth < VisibleArea.Width)
            {
                HideTooltip();
                DrawTooltip(oldTooltip, p, oldRect);
            }
        }

        /// <summary>
        /// Отрисовка Tooltip на Canvas
        /// </summary>
        void ShowTooltip(CanvasBarDrawManager.BarData bar, Point p)
        {
            var grid = new Grid();
            grid.Background = Brushes.LightGray;
            grid.Background.Freeze();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            var hasComment = !string.IsNullOrEmpty(bar.BarModel?.Leasing.Comment);
            if (hasComment)
                grid.RowDefinitions.Add(new RowDefinition());

            if (bar.BarModel == null)
            {
                NewStyledTooltipRow(grid, "NO MODEL", 0);
            }
            else
            {
                NewStyledTooltipRow(grid, bar.BarModel.Leasing.Title, 0);

                NewStyledTooltipRow(grid, bar.BarModel.CarName, 1);

                NewStyledTooltipRow(grid, GetDataSpan(bar.BarModel), 2);

                if (hasComment)
                {
                    NewStyledTooltipRow(grid, bar.BarModel.Leasing.Comment, 3);
                }

                if (!string.IsNullOrEmpty(bar.BarModel.Leasing.Saler))
                {
                    var tb = NewStyledTooltipRow(grid, bar.BarModel.Leasing.Saler, 4);
                    tb.HorizontalAlignment = HorizontalAlignment.Right;
                }
            }

            //force render для получения ActualHeight & ActualWidth
            grid.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            grid.Arrange(new Rect(grid.DesiredSize));

            DrawTooltip(grid, p, bar);
        }

        void DrawTooltip(Grid tooltip, Point p, CanvasBarDrawManager.BarData bar)
        {
            //расчёт выхода tooltip за правую границу контрола
            var x = p.X;
            var leftPoint = x + tooltip.ActualWidth + 2d;
            var va = VisibleArea;
            if (leftPoint > va.Width)
            {
                var diff = leftPoint - va.Width;

                x -= diff;
            }

            //расчёт выхода tooltip за нижнюю границу контрола
            var y = bar.VerticalOffset + bar.Border.Height + 3d;

            var botPoint = y + tooltip.ActualHeight;

            if (botPoint > va.Height)
            {
                y = va.Height - tooltip.ActualHeight - 20d; //20 - основной скролл
            }

            var dv = new DrawingVisual();
            var vb = new VisualBrush(tooltip);

            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(vb, null, new Rect(x, y, tooltip.ActualWidth, tooltip.ActualHeight));
            }

            m_tooltip = tooltip;
            m_TooltipedRect = bar;
            m_tooltipVisual = dv;
            m_chart.AddVisual(dv);
        }

        TextBlock NewStyledTooltipRow(Grid grid, string text, int index)
        {
            var tb = new TextBlock();
            tb.Margin = new Thickness(10d, 5d, 10d, 5d);
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            tb.Text = text;
            grid.Children.Add(tb);
            Grid.SetRow(tb, index);

            return tb;
        }

        /// <summary>
        /// Удаление Tooltip'а с Canvas
        /// </summary>
        public void HideTooltip()
        {
            if (m_tooltipVisual != null)
            {
                m_chart.Remove(m_tooltipVisual);
                m_tooltipVisual = null;
            }

            m_TooltipedRect = null;
            m_tooltip = null;
        }

        /// <summary>
        /// Получение строкового представления срока аренды
        /// </summary>
        /// <param name="model">Модель</param>
        /// <returns>Возвращает срок аренды</returns>
        string GetDataSpan(LeasingElementModel model)
        {
            //копипаста из BussinessDateConverter (старая версия)
            StringBuilder sb = new StringBuilder();
            //<действие> c XX по ХХ <месяц>
            sb.Append("в прокате ").Append(" c ");

            var b = model.Leasing;
            if (b.MonthCount < 2)
                sb.Append(b.DateStart.Day.ToString()).Append(" по ").Append(b.DateEnd.Day.ToString()).Append(" ").Append(b.DateStart.GetMonthName() ?? string.Empty);
            else
                sb.Append(b.DateStart.Day.ToString()).Append(" ").Append(b.DateStart.GetMonthName() ?? string.Empty).Append(" по ")
                    .Append(b.DateEnd.Day.ToString()).Append(" ").Append(b.DateEnd.GetMonthName() ?? string.Empty);

            return sb.ToString();
        }

        public void Dispose()
        {
            m_TooltipedRect = null;
            m_tooltip = null;
            m_chart = null;
            m_tooltipVisual = null;
        }
    }
}
