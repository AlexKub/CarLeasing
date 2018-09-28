using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static CarLeasingViewer.LeasingChart;

namespace CarLeasingViewer.Controls
{
    /// <summary>
    /// График занятости автомобилей
    /// </summary>
    public partial class LeasingChart : Canvas
    {
        CanvasGridDrawManager m_gridM;
        CanvasBarDrawManager m_barM;
        CanvasTextDrawManager m_textM;

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

                //var val = (double)e.NewValue;
                //if ((int)e.NewValue < 0) throw new ArgumentOutOfRangeException("Количество дней не может быть меньше 0");
                _this.m_gridM.DrawColumns((int)e.NewValue);
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

                var newVal = ((double)e.NewValue + 1); //+ 1: захардкожена ширина границы у колонки
                if (_this.m_gridM != null)
                {
                    _this.m_gridM.ColumnWidth = newVal;
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

                var newValue = (double)e.NewValue;
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

        public static DependencyProperty dp_Leasings = DependencyProperty.Register(nameof(Leasings), typeof(IEnumerable<Models.LeasingElementModel>), typeof(LeasingChart), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(IEnumerable<Models.LeasingElementModel>),
            PropertyChangedCallback = (s, e) =>
            {
                var _this = s as LeasingChart;
                var val = e.NewValue as IEnumerable<Models.LeasingElementModel>;

                if (val != null)
                {
                    if (_this.m_gridM != null)
                    {
                        var rowsI = val.Select(l => l.RowIndex).Distinct();
                        foreach (var i in rowsI)
                            _this.m_gridM.DrawRow(i);
                    }

                    if(_this.m_barM != null)
                    {
                        foreach (var bm in val)
                        {
                            _this.m_barM.DrawBar(bm);
                        }
                    }
                }

            }

        });
        /// <summary>
        /// Набор аренд авто
        /// </summary>
        public IEnumerable<Models.LeasingElementModel> Leasings { get { return (IEnumerable<Models.LeasingElementModel>)GetValue(dp_Leasings); } set { SetValue(dp_Leasings, value); } }

        public LeasingChart()
        {
            InitializeComponent();

            m_gridM = new CanvasGridDrawManager(this);
            m_barM = new CanvasBarDrawManager(this);
            m_textM = new CanvasTextDrawManager(this);

            base.Unloaded += LeasingChart_Unloaded;
        }


        protected override void OnRender(DrawingContext dc)
        {
            //отрисовываем текст для полосок на Canvas
            //if (m_barM != null)
            //    m_barM.DrawText(dc);


            base.OnRender(dc);
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

            if(m_textM != null)
            {
                m_textM.Dispose();
                m_textM = null;
            }
        }
    }
}
