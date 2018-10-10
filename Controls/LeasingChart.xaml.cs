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

        #region Managers

        /// <summary>
        /// Управление отрисовки сетки
        /// </summary>
        CanvasGridDrawManager m_gridM;
        /// <summary>
        /// Управление отрисовкой прямоугольников на графике
        /// </summary>
        CanvasBarDrawManager m_barM;
        /// <summary>
        /// Управление отрисовкой текста на графике
        /// </summary>
        CanvasTextDrawManager m_textM;
        /// <summary>
        /// Управление отрисовкой Layout'ов строк
        /// </summary>
        CanvasRowLayoutDrawManager m_rowLayoutM;
        /// <summary>
        /// Управление абстракциями строк
        /// </summary>
        RowManager m_rowM;
        /// <summary>
        /// Управление подсветкой
        /// </summary>
        HightlightManager m_hightlightM;

        /// <summary>
        /// Отрисовка прямоугольников на графике
        /// </summary>
        public CanvasBarDrawManager BorderDrawer { get { return m_barM; } }

        /// <summary>
        /// Отрисовка текса на графике
        /// </summary>
        public CanvasTextDrawManager TextDrawer { get { return m_textM; } }

        /// <summary>
        /// Layout'ы строк на графике
        /// </summary>
        public CanvasRowLayoutDrawManager RowLayoutDrawer { get { return m_rowLayoutM; } }

        /// <summary>
        /// Строки на графике
        /// </summary>
        public RowManager RowManager { get { return m_rowM; } }

        #endregion

        DrawingVisual m_tooltip;
        CanvasBarDrawManager.BarData m_TooltipedRect;

        /// <summary>
        /// При изменении Набора аренды
        /// </summary>
        public event LeasingSetEvent SetChanged;

        public static DependencyProperty dp_DayCount = DependencyProperty.Register(nameof(DayCount), typeof(int), typeof(LeasingChart), new FrameworkPropertyMetadata() { DefaultValue = default(int) });
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

                if (_this.m_rowLayoutM != null)
                    _this.m_rowLayoutM.RowHeight = newValue;
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

        LeasingSet m_set;
        /// <summary>
        /// Текущий набор данных
        /// </summary>
        public LeasingSet LeasingSet
        {
            get { return m_set; }
            set
            {
                if (value != m_set)
                {
                    var e = new LeasingSetEventArgs(value, m_set);

                    m_set = value;

                    SetChanged?.Invoke(e);
                }
            }
        }

        public LeasingChart()
        {
            InitializeComponent();

            m_gridM = new CanvasGridDrawManager(this);
            m_barM = new CanvasBarDrawManager(this);
            m_textM = new CanvasTextDrawManager(this);
            m_rowLayoutM = new CanvasRowLayoutDrawManager(this);
            //важно!!! подписывать крайним - зависим (подписывается) от других
            m_rowM = new RowManager(this);
            m_hightlightM = new HightlightManager(this);

            base.Unloaded += LeasingChart_Unloaded;

            m_children = new VisualCollection(this);
        }

        /// <summary>
        /// Отрисовка данных на графике
        /// </summary>
        public void Draw()
        {
            m_children.Clear();

            ClearManagers();
            DrawingVisual dv = null;

            var rowsI = Leasings.Select(l => l.RowIndex).Distinct();

            //отрисовка Layout'ов для строк графика
            if (m_rowLayoutM != null)
            {
                foreach (var i in rowsI)
                {
                    dv = m_rowLayoutM.DrawRowLayout(i);

                    if (dv != null)
                        m_children.Add(dv);
                }
            }

            //отрисовка сетки
            if (m_gridM != null)
            {
                foreach (var i in rowsI)
                {
                    dv = m_gridM.DrawRow(i);
                    if (dv != null)
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

            //отрисовка прямоугольников и текста
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

        #region FrameworkElement

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

        #endregion

        void ClearManagers()
        {
            HideTooltip();
            m_gridM.Clear();
            m_textM.Clear();
            m_barM.Clear();
            m_rowLayoutM.Clear();
            m_hightlightM.Clear();
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

            if (m_rowLayoutM != null)
            {
                m_rowLayoutM.Dispose();
                m_rowLayoutM = null;
            }

            if(m_hightlightM != null)
            {
                m_hightlightM.Dispose();
                m_hightlightM = null;
            }

            if(m_rowM != null)
            {
                m_rowM.Dispose();
                m_rowM = null;
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            HideTooltip();

            //снимаем подсветку при наведении при выходе мыши за границы контрола
            m_hightlightM.Hightlight(-1, HightlightManager.HightlightAction.Hightlight);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var point = e.GetPosition(this);

            //получаем Layout, над которым находится мышь
            var rLayout = m_rowLayoutM.Contains(point);
            //индекс строки, над которой сейчас находится курсор мыши
            var rowIndex = rLayout == null ? -1 : rLayout.RowIndex;

            //подсвечиваем элементы, над которыми находится мышь
            m_hightlightM.Hightlight(rowIndex, rowIndex == -1 ? HightlightManager.HightlightAction.None : HightlightManager.HightlightAction.Hightlight);

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
                if (leftPoint > ActualWidth)
                {
                    var diff = leftPoint - ActualWidth;

                    x -= diff;
                }

                //расчёт выхода tooltip за нижнюю границу контрола
                var y = bar.VerticalOffset + bar.Border.Height + 3d;

                var botPoint = y + grid.ActualHeight + 20d;
                
                if (botPoint > ActualHeight)
                {
                    var diff = botPoint - grid.ActualHeight - RowHeight - 20d; //20 - основной скролл чуть больше видимого
                    y -= diff;
                }

                dc.DrawRectangle(vb, null, new Rect(x, y, grid.ActualWidth, grid.ActualHeight));
                m_TooltipedRect = bar;
            }
            m_children.Add(dv);
            m_tooltip = dv;
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
