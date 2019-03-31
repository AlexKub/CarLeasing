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
                if (m_TooltipedRect.Bar.Contains(point))
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
                    if (b.Bar.Contains(point))
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
            //grid.RowDefinitions.Add(new RowDefinition());

            if (bar.Model == null)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                NewStyledTooltipRow(grid, "NO MODEL", 0);
            }
            else
            {
                var tooltipRows = bar.Model.ToolTipRows;
                if (tooltipRows == null || tooltipRows.Length == 0)
                {
                    grid.RowDefinitions.Add(new RowDefinition());
                    NewStyledTooltipRow(grid, "NO TOOLTIP INFO", 0);
                }
                else
                {
                    for (int i = 0; i < tooltipRows.Length; i++)
                    {
                        grid.RowDefinitions.Add(new RowDefinition());
                        NewStyledTooltipRow(grid, tooltipRows[i], i);
                    }
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
            var y = bar.VerticalOffset + m_chart.RowHeight + 3d;

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
        string GetDataSpan(LeasingBarModel model)
        {
            //копипаста из BussinessDateConverter (старая версия)
            StringBuilder sb = new StringBuilder();
            //<действие> c XX по ХХ <месяц>
            string inline = "    ";
            sb.AppendLine("В аренде:").Append(inline).Append("c ");

            var b = model.Leasing;
            //if (b.MonthCount < 2) //для одного месяца
            //    sb.Append(b.DateStart.Day.ToString())
            //        //.Append(b.DateStart.Year < b.DateEnd.Year ? (b.DateStart.Year.ToString() + " ") : string.Empty)
            //        .Append(" ")
            //        .Append(b.DateEnd.Day.ToString()).Append(" ")
            //        //.Append(b.DateStart.Month == b.DateEnd.Month ? "" : b.DateStart.GetMonthName() + " ")
            //        .AppendLine(b.DateStart.TimeOfDay.Hours > 0 ? b.DateStart.TimeOfDay.ToString(@"hh\:mm") : string.Empty)
            //        .Append(inline)
            //        .Append("по ")
            //        .Append(b.DateEnd.Day.ToString())
            //        .Append(" ")
            //        .Append(b.DateEnd.GetMonthName()).Append(" ")
            //        //.Append(b.DateStart.Year < b.DateEnd.Year ? (b.DateEnd.Year.ToString() + " ") : string.Empty)
            //        .Append(b.DateEnd.TimeOfDay.Hours > 0 ? b.DateEnd.TimeOfDay.ToString(@"hh\:mm") : string.Empty).Append(" ");
            //else //для нескольких месяцев 
                sb.Append(b.DateStart.Day.ToString()).Append(" ")
                    .Append(b.DateStart.GetMonthName()).Append(" ")
                    .Append(b.DateStart.Year.ToString()).Append(" ")
                    .AppendLine(b.DateStart.TimeOfDay.Hours > 0 ? b.DateStart.TimeOfDay.ToString(@"hh\:mm") : string.Empty)
                    .Append(inline)
                    .Append("по ")
                    .Append(b.DateEnd.Day.ToString()).Append(" ")
                    .Append(b.DateEnd.GetMonthName()).Append(" ")
                    .Append(b.DateEnd.Year.ToString()).Append(" ")
                    .Append(b.DateEnd.TimeOfDay.Hours > 0 ? b.DateEnd.TimeOfDay.ToString(@"hh\:mm") : string.Empty);

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
