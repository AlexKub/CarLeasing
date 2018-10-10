using CarLeasingViewer.Controls.LeasingChartManagers;
using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CarLeasingViewer.Controls
{
    /// <summary>
    /// График занятости автомобилей
    /// </summary>
    public partial class LeasingChart : FrameworkElement
    {
        // Create a collection of child visual objects.
        private readonly VisualCollection m_children;

        CanvasGridDrawManager m_gridM;
        CanvasBarDrawManager m_barM;
        CanvasTextDrawManager m_textM;
        DrawingVisual m_tooltip;
        CanvasBarDrawManager.BarData m_TooltipedRect;

        public CanvasBarDrawManager BorderDrawer { get { return m_barM; } }

        public CanvasTextDrawManager TextDrawer { get { return m_textM; } }

        public static DependencyProperty dp_DayCount = DependencyProperty.Register(nameof(DayCount), typeof(int), typeof(LeasingChart), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(int),
            PropertyChangedCallback = (s, e) =>
            {

                var _this = s as LeasingChart;

                if (_this == null)
                    return;

                if (_this.m_gridM == null)
                    return;

                //_this.m_gridM.DrawColumns((int)e.NewValue);
            }
        });
        /// <summary>
        /// Суммарное количество дней, отображаемое на графике
        /// </summary>
        public int DayCount { get { return (int)GetValue(dp_DayCount); } set { SetValue(dp_DayCount, value); } }

        public static DependencyProperty dp_DayColumnWidth = DependencyProperty.Register(nameof(DayColumnWidth), typeof(double), typeof(LeasingChart), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(double),
            PropertyChangedCallback = (s, e) =>
            {
                var _this = s as LeasingChart;

                if (_this == null)
                    return;

                var newVal = ((double)e.NewValue); 
                if (_this.m_gridM != null)
                {
                    _this.m_gridM.ColumnWidth = newVal + 1; //+ 1: захардкожена ширина границы у колонки
                }
                if (_this.m_barM != null)
                {
                    _this.m_barM.DayColumnWidth = newVal;
                }
            }
        });
        /// <summary>
        /// Ширина колонки одного дня
        /// </summary>
        public double DayColumnWidth { get { return (double)GetValue(dp_DayColumnWidth); } set { SetValue(dp_DayColumnWidth, value); } }

        public static DependencyProperty dp_LineBrush = DependencyProperty.Register(nameof(LineBrush), typeof(Brush), typeof(LeasingChart), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(Brush),
            PropertyChangedCallback = (s, e) =>
            {
                var _this = s as LeasingChart;

                if (_this == null)
                    return;

                var newVal = e.NewValue as Brush;
                if (_this.m_gridM != null)
                {
                    _this.m_gridM.LineBrush = newVal;
                }
            }
        });
        /// <summary>
        /// Кисть для линий сетки
        /// </summary>
        public Brush LineBrush { get { return (Brush)GetValue(dp_LineBrush); } set { SetValue(dp_LineBrush, value); } }

        public static DependencyProperty dp_BarBrush = DependencyProperty.Register(nameof(BarBrush), typeof(Brush), typeof(LeasingChart), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(Brush),
            PropertyChangedCallback = (s, e) =>
            {
                var _this = s as LeasingChart;

                if (_this == null)
                    return;

                if (_this.m_barM == null)
                    return;

                _this.m_barM.BackgroundBrush = e.NewValue as Brush;
            }
        });
        /// <summary>
        /// Кисть для заливки панелей на графике
        /// </summary>
        public Brush BarBrush { get { return (Brush)GetValue(dp_BarBrush); } set { SetValue(dp_BarBrush, value); } }

        public static DependencyProperty dp_BarBorderBrush = DependencyProperty.Register(nameof(BarBorderBrush), typeof(Brush), typeof(LeasingChart), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(Brush),
            PropertyChangedCallback = (s, e) =>
            {
                var _this = s as LeasingChart;

                if (_this == null)
                    return;

                if (_this.m_barM == null)
                    return;

                _this.m_barM.BorderBrush = e.NewValue as Brush;
            }
        });
        public Brush BarBorderBrush { get { return (Brush)GetValue(dp_BarBorderBrush); } set { SetValue(dp_BarBorderBrush, value); } }

        public static DependencyProperty dp_Font = DependencyProperty.Register(nameof(FontFamily), typeof(FontFamily), typeof(LeasingChart), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(FontFamily),
            PropertyChangedCallback = (s, e) =>
            {
                var _this = s as LeasingChart;

                if (_this == null)
                    return;

                if (_this.m_textM != null)
                {
                    _this.m_textM.FontFamily = e.NewValue as FontFamily;
                }
            }
        });
        public FontFamily FontFamily { get { return (FontFamily)GetValue(dp_Font); } set { SetValue(dp_Font, value); } }

        public static DependencyProperty dp_FontSize = DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(LeasingChart), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(double),
            PropertyChangedCallback = (s, e) =>
            {
                var _this = s as LeasingChart;

                if (_this == null)
                    return;

                if (_this.m_textM != null)
                {
                    _this.m_textM.FontSize = (double)e.NewValue;
                }
            }
        });
        public double FontSize { get { return (double)GetValue(dp_FontSize); } set { SetValue(dp_FontSize, value); } }

        public static DependencyProperty dp_RowHeight = DependencyProperty.Register(nameof(RowHeight), typeof(double), typeof(LeasingChart), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(double),
            PropertyChangedCallback = (s, e) =>
            {
                var _this = s as LeasingChart;

                var newValue = (double)e.NewValue + 1d;
                if (_this.m_gridM != null)
                    _this.m_gridM.RowHeight = newValue;

                if (_this.m_barM != null)
                    _this.m_barM.RowHeight = newValue;
            }
        });
        /// <summary>
        /// Высота строк на графике
        /// </summary>
        public double RowHeight { get { return (double)GetValue(dp_RowHeight); } set { SetValue(dp_RowHeight, value); } }

        public static DependencyProperty dp_Leasings = DependencyProperty.Register(nameof(Leasings), typeof(IEnumerable<Models.LeasingElementModel>), typeof(LeasingChart), new FrameworkPropertyMetadata() { DefaultValue = new List<LeasingElementModel>() });
        /// <summary>
        /// Набор аренд авто
        /// </summary>
        public IEnumerable<LeasingElementModel> Leasings { get { return (IEnumerable<Models.LeasingElementModel>)GetValue(dp_Leasings); } set { SetValue(dp_Leasings, value); } }

        /// <summary>
        /// Текущий набор данных
        /// </summary>
        public LeasingSet LeasingSet { get; set; }

        public LeasingChart()
        {
            InitializeComponent();

            m_gridM = new CanvasGridDrawManager(this);
            m_barM = new CanvasBarDrawManager(this);
            m_textM = new CanvasTextDrawManager(this);

            base.Unloaded += LeasingChart_Unloaded;

            m_children = new VisualCollection(this);
        }

        public void Draw()
        {
            m_children.Clear();

            ClearManagers();
            DrawingVisual dv = null;

            //отрисовка сетки
            if (m_gridM != null)
            {
                var rowsI = Leasings.Select(l => l.RowIndex).Distinct();
                foreach (var i in rowsI)
                {
                    dv = m_gridM.DrawRow(i);
                    if(dv != null)
                        m_children.Add(dv); //строки
                }

                //колонки
                var colCount = DayCount + 1;
                for (int i = 1; i < colCount; i++)
                {
                    dv = m_gridM.DrawColumn(i);
                    if (dv != null)
                        m_children.Add(dv);
                }
                
            }

            if (m_barM != null && m_textM != null)
            {
                foreach (var bm in Leasings)
                {
                    dv = m_barM.DrawBar(bm);

                    if (dv != null)
                        m_children.Add(dv);

                    dv = m_textM.DrawText(bm);

                    if (dv != null)
                        m_children.Add(dv);
                }
            }
        }

        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount => m_children.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= m_children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return m_children[index];
        }

        void ClearManagers()
        {
            HideTooltip();
            m_gridM.Clear();
            m_textM.Clear();
            m_barM.Clear();
        }

        private void LeasingChart_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= LeasingChart_Unloaded;

            if (m_gridM != null)
            {
                m_gridM.Dispose();
                m_gridM = null;
            }

            if (m_barM != null)
            {
                m_barM.Dispose();
                m_barM = null;
            }

            if (m_textM != null)
            {
                m_textM.Dispose();
                m_textM = null;
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            HideTooltip();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var point = e.GetPosition(this);

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
            foreach (var kvp in m_barM.Data)
            {
                bar = kvp.Value;
                if (bar.VerticalOffset <= point.Y)
                {
                    if (bar.Border.Contains(point))
                    {
                        HideTooltip();

                        DrawTooltip(bar, point);
                        m_TooltipedRect = bar; //сохраняем найденный элемент
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Отрисовка Tooltip на Canvas
        /// </summary>
        void DrawTooltip(CanvasBarDrawManager.BarData bar, Point p)
        {
            var grid = new Grid();
            grid.Background = Brushes.LightGray;
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            TextBlock text0 = null;
            if (bar.BarModel == null)
            {
                text0 = NewStyledTooltipRow();
                text0.Text = "NO MODEL";
            }
            else
            {
                text0 = NewStyledTooltipRow();
                text0.Text = bar.BarModel.Leasing.Title;

                var text1 = NewStyledTooltipRow();
                text1.Text = bar.BarModel.CarName;

                var text2 = NewStyledTooltipRow();
                text2.Text = GetDataSpan(bar.BarModel);

                grid.Children.Add(text1);
                grid.Children.Add(text2);
                Grid.SetRow(text1, 1);
                Grid.SetRow(text2, 2);
            }

            grid.Children.Add(text0);

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
                if (leftPoint > ActualWidth)
                {
                    var diff = leftPoint - ActualWidth;

                    x -= diff;
                }

                //расчёт выхода tooltip за нижнюю границу контрола
                var y = bar.VerticalOffset + bar.Border.Height + 3d;
                var botPoint = y + grid.ActualHeight + 2d;
                if(botPoint > ActualHeight)
                {
                    var diff = botPoint - ActualHeight - RowHeight;
                    y -= diff;
                }

                dc.DrawRectangle(vb, null, new Rect(x, y, grid.ActualWidth, grid.ActualHeight));
                m_TooltipedRect = bar;
            }
            m_children.Add(dv);
            m_tooltip = dv;
        }

        TextBlock NewStyledTooltipRow()
        {
            var tb = new TextBlock();
            tb.Margin = new Thickness(5);
            tb.HorizontalAlignment = HorizontalAlignment.Center;

            return tb;
        }

        /// <summary>
        /// Удаление Tooltip'а с Canvas
        /// </summary>
        void HideTooltip()
        {
            if (m_tooltip != null)
            {
                m_children.Remove(m_tooltip);
                //Children.Remove(m_tooltip);
                m_tooltip = null;
            }

            //сбрасываем подсвеченный элемент
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
    }
}
