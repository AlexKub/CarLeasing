﻿using CarLeasingViewer.Controls.LeasingChartManagers;
using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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

        /// <summary>
        /// Видимая площадь для рисования
        /// </summary>
        public VisibleArea VisibleArea { get { return m_tooltipM == null ? new VisibleArea() : m_tooltipM.VisibleArea; } set { m_tooltipM.VisibleArea = value; } }

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
        CanvasColumnLayoutDrawManager m_columnLayoutM;
        /// <summary>
        /// Управление абстракциями строк
        /// </summary>
        RowManager m_rowM;
        ColumnManager m_columnM;
        /// <summary>
        /// Управление подсветкой
        /// </summary>
        HightlightManager m_hightlightM;
        /// <summary>
        /// Управление Tooltip'ом
        /// </summary>
        TooltipManager m_tooltipM;

        /// <summary>
        /// Отрисовка прямоугольников на графике
        /// </summary>
        public CanvasBarDrawManager BarManager { get { return m_barM; } }

        /// <summary>
        /// Отрисовка текса на графике
        /// </summary>
        public CanvasTextDrawManager TextDrawer { get { return m_textM; } }

        /// <summary>
        /// Layout'ы строк на графике
        /// </summary>
        public CanvasRowLayoutDrawManager RowLayoutDrawer { get { return m_rowLayoutM; } }
        public CanvasColumnLayoutDrawManager ColumnLayoutDrawer { get { return m_columnLayoutM; } }

        /// <summary>
        /// Строки на графике
        /// </summary>
        public RowManager RowManager { get { return m_rowM; } }
        public ColumnManager ColumnManager { get { return m_columnM; } }

        public HightlightManager HightlightManager { get { return m_hightlightM; } }

        #endregion

        /// <summary>
        /// При изменении Набора аренды
        /// </summary>
        public event LeasingSetEvent SetChanged;
        /// <summary>
        /// При выборе строки
        /// </summary>
        public event RowSelectedEvent RowSelectionChanged;

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
                    _this.m_gridM.ColumnWidth = newVal + 1d; //+ 1: захардкожена ширина границы у колонки
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

        public static DependencyProperty dp_LineBoldBrush = DependencyProperty.Register(nameof(LineBoldBrush), typeof(Brush), typeof(LeasingChart), new FrameworkPropertyMetadata()
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
                    _this.m_gridM.LineBoldBrush = newVal;
                }
            }
        });
        /// <summary>
        /// Кисть для толстых линий сетки
        /// </summary>
        public Brush LineBoldBrush { get { return (Brush)GetValue(dp_LineBoldBrush); } set { SetValue(dp_LineBoldBrush, value); } }

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

        public static readonly DependencyProperty dp_BlockeBarBrushProperty = DependencyProperty.Register(nameof(BlockeBarBrush), typeof(Brush), typeof(LeasingChart), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(Brush),
            PropertyChangedCallback = (s, e) =>
            {
                var _this = s as LeasingChart;

                if (_this == null)
                    return;

                if (_this.m_barM == null)
                    return;

                _this.m_barM.BlockedBarBrush = e.NewValue as Brush;
            }
        });
        /// <summary>
        /// Кисть для заблокированных
        /// </summary>
        public Brush BlockeBarBrush { get { return (Brush)GetValue(dp_BlockeBarBrushProperty); } set { SetValue(dp_BlockeBarBrushProperty, value); } }

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
            m_columnLayoutM = new CanvasColumnLayoutDrawManager(this);
            //важно!!! подписывать крайним - зависим (подписывается) от других
            m_rowM = new RowManager(this);
            m_columnM = new ColumnManager(this);
            m_hightlightM = new HightlightManager(this);
            m_tooltipM = new TooltipManager(this);

            base.Unloaded += LeasingChart_Unloaded;

            m_children = new VisualCollection(this);

            Subscribe(true);
        }

        /// <summary>
        /// Отрисовка данных на графике
        /// </summary>
        public void Draw()
        {
            m_children.Clear();

            ClearManagers();
            DrawingVisual dv = null;

            var rowsI = Leasings.Select(l => l.RowIndex).Distinct().OrderBy(i => i);

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

            RedrawGrid(rowsI);

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

        public void ClearManagers()
        {
            m_tooltipM.HideTooltip();
            m_gridM.Clear();
            m_textM.Clear();
            m_barM.Clear();
            m_rowLayoutM.Clear();
            m_hightlightM.Clear();
            m_rowM.Clear();
        }

        void Subscribe(bool subscribe)
        {
            if (subscribe)
            {
                m_rowM.RowSelectionChanged += M_rowM_RowSelectionChanged;
            }
            else
            {
                m_rowM.RowSelectionChanged -= M_rowM_RowSelectionChanged;
            }
        }

        private void M_rowM_RowSelectionChanged(Row row)
        {
            RowSelectionChanged?.Invoke(row);
        }

        private void LeasingChart_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= LeasingChart_Unloaded;

            Subscribe(false);

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

            if (m_hightlightM != null)
            {
                m_hightlightM.Dispose();
                m_hightlightM = null;
            }

            if (m_rowM != null)
            {
                m_rowM.Dispose();
                m_rowM = null;
            }

            if (m_tooltipM != null)
            {
                m_tooltipM.Dispose();
                m_tooltipM = null;
            }
        }

        /// <summary>
        /// Перерисовка сетки
        /// </summary>
        public void RedrawGrid(IEnumerable<int> rowsI = null)
        {
            if (rowsI == null)
                rowsI = Leasings.Select(l => l.RowIndex).Distinct().OrderBy(i => i);

            //для случаев, когда меняется размер контрола, а сетка остаётся прежней
            if (m_gridM != null)
            {
                DrawingVisual dv = null;

                //отрисовка строк сетки
                foreach (var i in rowsI)
                {
                    dv = m_gridM.DrawRow(i);
                    if (dv != null)
                        if (!m_children.Contains(dv))
                            m_children.Add(dv); //строки
                }
                //отрисовка колонок сетки
                var colCount = DayCount + 1;

                var monthDayCounts = LeasingSet.Monthes?.Select(m => m.Month.DayCount).ToList() ?? new List<int>();

                var monthIndex = 0;
                var monthBorderIndex = monthDayCounts.Count > 0 ? monthDayCounts[monthIndex] : 0;

                bool boldLine = false;
                for (int i = 1; i < colCount; i++)
                {
                    if (i == monthBorderIndex)
                    {
                        boldLine = true;

                        monthIndex++;

                        if (monthDayCounts.Count > monthIndex)
                        {
                            monthBorderIndex = monthBorderIndex + monthDayCounts[monthIndex];
                        }
                    }
                    else
                    {
                        boldLine = false;
                    }

                    dv = m_gridM.DrawColumn(i, rowsI?.Count() ?? 0, boldLine);
                    if (dv != null)
                        if (!m_children.Contains(dv))
                            m_children.Add(dv);
                }
            }
        }

        #region Mouse handlers

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            m_tooltipM.HideTooltip();
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            var point = e.GetPosition(this);

            //получаем Layout, над которым находится мышь
            var rLayout = m_rowLayoutM.Contains(point);

            m_hightlightM.UnSelect(rLayout == null ? -1 : rLayout.RowIndex);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            var point = e.GetPosition(this);

            //получаем Layout, над которым находится мышь
            var rLayout = m_rowLayoutM.Contains(point);

            var rowIndex = rLayout == null ? -1 : rLayout.RowIndex;
            m_hightlightM.Select(rowIndex);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var point = e.GetPosition(this);

            //получаем Layout, над которым находится мышь
            var rLayout = m_rowLayoutM.Contains(point);

            //подсвечиваем наведённую строку
            m_hightlightM.Hightlight(rLayout == null ? -1 : rLayout.RowIndex);

            m_tooltipM.HandleTooltip(point);
        }

        #endregion

        public void AddVisual(DrawingVisual visual)
        {
            m_children.Add(visual);
        }

        public void Remove(DrawingVisual visual)
        {
            m_children.Remove(visual);
        }
    }
}
