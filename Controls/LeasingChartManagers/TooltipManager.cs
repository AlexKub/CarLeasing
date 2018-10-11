using CarLeasingViewer.Models;
using System;
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
        DrawingVisual m_tooltip;
        CanvasBarDrawManager.BarData m_TooltipedRect;

        public TooltipManager(LeasingChart chart) { m_chart = chart; }

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
                    return;
                else //если мышь ушла с элемента
                {
                    //скрываем подсказку
                    HideTooltip();
                }
            }

            //поиск элемента, над которым сейчас находится мышь
            CanvasBarDrawManager.BarData bar = null;
            foreach (var kvp in m_chart.BorderDrawer.Data)
            {
                bar = kvp.Value;
                if (bar.VerticalOffset <= point.Y)
                {
                    if (bar.Border.Contains(point))
                    {
                        HideTooltip();

                        ShowTooltip(bar, point);
                        m_TooltipedRect = bar; //сохраняем найденный элемент
                        return;
                    }
                }
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
            }

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                var vb = new VisualBrush(grid);
                //force render
                grid.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                grid.Arrange(new Rect(grid.DesiredSize));

                //расчёт выхода tooltip за правую границу контрола
                var x = p.X;
                var leftPoint = x + grid.ActualWidth + 2d;
                if (leftPoint > m_chart.ActualWidth)
                {
                    var diff = leftPoint - m_chart.ActualWidth;

                    x -= diff;
                }

                //расчёт выхода tooltip за нижнюю границу контрола
                var y = bar.VerticalOffset + bar.Border.Height + 3d;

                var botPoint = y + grid.ActualHeight + 20d;

                if (botPoint > m_chart.ActualHeight)
                {
                    var diff = botPoint - grid.ActualHeight - m_chart.RowHeight - 20d; //20 - основной скролл чуть больше видимого
                    y -= diff;
                }

                dc.DrawRectangle(vb, null, new Rect(x, y, grid.ActualWidth, grid.ActualHeight));
                m_TooltipedRect = bar;
            }

            m_tooltip = dv;
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
            if (m_tooltip != null)
            {
                m_chart.Remove(m_tooltip);
                m_tooltip = null;
            }

            m_TooltipedRect = null;
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
        }
    }
}
