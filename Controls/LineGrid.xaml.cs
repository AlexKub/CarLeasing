using CarLeasingViewer.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CarLeasingViewer.Controls
{
    /// <summary>
    /// Interaction logic for LineGrid.xaml
    /// </summary>
    public partial class LineGrid : UserControl
    {
        const string RowConverterName = "RowConverter";
        const string ColumnConverterName = "ColumnConverter";
        internal static readonly LineGrid DefaultControlReference = new LineGrid();

        /// <summary>
        /// Копия коллекции ItemsSource
        /// </summary>
        List<double> m_items = new List<double>();

        #region Dependency properties

        public static DependencyProperty dp_ItemsControlActualWidth = DependencyProperty.Register(nameof(ItemsControlActualWidth), typeof(double), typeof(LineGrid), new FrameworkPropertyMetadata() { DefaultValue = default(double) });
        public double ItemsControlActualWidth { get { return (double)GetValue(dp_ItemsControlActualWidth); } set { SetValue(dp_ItemsControlActualWidth, value); } }

        public static DependencyProperty dp_Rows = DependencyProperty.Register(nameof(Rows), typeof(IList<GridLineValue<double>>), typeof(LineGrid), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(IList<GridLineValue<double>>)
            ,
            PropertyChangedCallback = (s, e) =>
            {
                var collection = e.NewValue as IList<GridLineValue<double>>;
                if(collection != null && collection.Count > 0)
                {
                    for (int i = 0; i < collection.Count; i++)
                        collection[i].ParentControlRef = s as LineGrid;
                }
            }
        });
        public IList<GridLineValue<double>> Rows { get { return (IList<GridLineValue<double>>)GetValue(dp_Rows); } set { SetValue(dp_Rows, value); } }

        public static DependencyProperty dp_Month = DependencyProperty.Register(nameof(Month), typeof(Month), typeof(LineGrid), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(Month)
            ,
            PropertyChangedCallback = (s, e) =>
            {
                var month = (e.NewValue as Month);

                if (month != null)
                {
                    //добавляем на одну линию больше, как закрывающую
                    //на текущий момент биндится только количество значений (сами значения коллекции не используются)
                    var columnLines = month.DayIndexes;
                    columnLines.Add(month.DayCount + 1);
                    (s as LineGrid).DrawColumns(columnLines);
                }
            }
        });
        public Month Month { get { return (Month)GetValue(dp_Month); } set { SetValue(dp_Month, value); } }

        public static DependencyProperty dp_Columns = DependencyProperty.Register(nameof(Columns), typeof(ObservableCollection<GridLineValue<int>>), typeof(LineGrid), new FrameworkPropertyMetadata()
        {
            DefaultValue = new ObservableCollection<GridLineValue<int>>()
            ,
            PropertyChangedCallback = (s, e) => { (s as LineGrid).ResetColumnConverter(); }
        });
        public ObservableCollection<GridLineValue<int>> Columns { get { return (ObservableCollection<GridLineValue<int>>)GetValue(dp_Columns); } set { SetValue(dp_Columns, value); } }

        public static DependencyProperty dp_LineBrush = DependencyProperty.Register(nameof(LineBrush), typeof(Brush), typeof(LineGrid), new FrameworkPropertyMetadata() { DefaultValue = default(Brush) });
        public Brush LineBrush { get { return (Brush)GetValue(dp_LineBrush); } set { SetValue(dp_LineBrush, value); } }

        public static DependencyProperty dp_LineWidth = DependencyProperty.Register(nameof(LineWidth), typeof(double), typeof(LineGrid), new FrameworkPropertyMetadata() { DefaultValue = default(double) });
        public double LineWidth { get { return (double)GetValue(dp_LineWidth); } set { SetValue(dp_LineWidth, value); } }

        public static DependencyProperty dp_LastRowY = DependencyProperty.Register(nameof(LastRowY), typeof(double), typeof(LineGrid), new FrameworkPropertyMetadata() { DefaultValue = default(double) });
        /// <summary>
        /// Координата Y для крайней строки
        /// </summary>
        public double LastRowY { get { return (double)GetValue(dp_LastRowY); } set { SetValue(dp_LastRowY, value); } }

        public static readonly DependencyProperty dp_OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(GridOrientation), typeof(LineGrid), new FrameworkPropertyMetadata() { DefaultValue = default(GridOrientation) });
        public GridOrientation Orientation { get { return (GridOrientation)GetValue(dp_OrientationProperty); } set { SetValue(dp_OrientationProperty, value); } }

        #endregion

        public LineGrid()
        {
            InitializeComponent();

            InitConverters();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == dp_Rows)
            {
                //если изменилась коллекция
                //сбрасываем индекс расчёта строки в начало
                ResetRowConverter();

                DrawRows((e.NewValue as IEnumerable<GridLineValue<double>>).Select(v => v.Value));

                //перерисовываем колонки, т.к. крайняя строка поменялась а месяц не изменился
                DrawColumns(Columns.Select(d => d.Value).ToList());
            }
            if (e.Property == DataContextProperty)
            {
                var context = DataContext as Month;

                if (context != null)
                    DrawColumns(context.DayIndexes);
            }

            base.OnPropertyChanged(e);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            ItemsControlActualWidth = ActualWidth;
        }

        public double GetRowHeight(int rowIndex)
        {
            /*
             * Высота строк расчитывается из:
             * 1. Высоты строки в контроле MapedItemsContainer после его рендеринга, полученной через binding коллекции
             * 2. + суммы всех высот строк перед ней
             */
            double height = 0d;

            for (int i = 0; i < rowIndex && i < m_items.Count; i++)
                //суммируем!!!
                //иначе, при умножении, появляется видимая погрешность (хреново шарп умножает)
                height += m_items[i];

            return height;
        }

        void ResetRowConverter()
        {
            var rowConverter = Resources[RowConverterName] as Converters.LineRowConverter;

            if (rowConverter != null)
                rowConverter.Reset();
        }

        void ResetColumnConverter()
        {
            var columnConverter = Resources[ColumnConverterName] as Converters.LineColumnConverter;

            if (columnConverter != null)
                columnConverter.Reset();
        }

        void InitConverters()
        {
            //даём конвертеру ссылку на текущий контрол
            //для вызова на нём логики расчёта высоты строки

            //сделал так, потому что в конвертер приходит высота строки
            //а в контроле хранится коллекция высот других строк
            var rowXConverter = Resources[RowConverterName] as Converters.LineRowConverter;

            if (rowXConverter != null)
                rowXConverter.ParentCollection = this;

        }

        void DrawRows(IEnumerable<double> heightCollection)
        {
            if (!HasOrientation(GridOrientation.Horizontal))
                return;

            m_items.Clear();

            if (heightCollection != null)
            {
                m_items.AddRange(heightCollection);

                LastRowY = GetRowHeight(m_items.Count); //пофиг, что индекс выходит за границу - там есть проверки
            }
        }

        void DrawColumns(IList<int> indexes)
        {
            if (!HasOrientation(GridOrientation.Vertical))
                return;

            ObservableCollection<GridLineValue<int>> oldColumns = null;

            if (Columns != null && Columns.Count > 0)
                oldColumns = Columns;

            if (indexes != null && indexes.Count > 0)
            {
                Columns = new ObservableCollection<GridLineValue<int>>(indexes.Select(i => new GridLineValue<int>(this, i)));
            }
            else
                Columns?.Clear();

            if (oldColumns != null)
                oldColumns.Clear();
        }

        private bool HasOrientation(GridOrientation orient)
        {
            return (orient & Orientation) > 0;
        }

        /// <summary>
        /// Значения для линий сетки
        /// </summary>
        [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
        public class GridLineValue<T>
        {
            LineGrid m_parentControlRef = LineGrid.DefaultControlReference;

            /// <summary>
            /// Ссылка на родительский контрол для избежания лишних RelativeSource binding'ов
            /// </summary>
            public LineGrid ParentControlRef
            {
                get { return m_parentControlRef; }
                set { m_parentControlRef = value ?? LineGrid.DefaultControlReference; }
            }

            /// <summary>
            /// Смещение для линии
            /// </summary>
            public T Value { get; private set; }

            public GridLineValue() : this(LineGrid.DefaultControlReference, default(T)) { }

            public GridLineValue(LineGrid controlRef) : this(controlRef, default(T)) { }

            public GridLineValue(LineGrid controlRef, T value) 
            {
                m_parentControlRef = controlRef ?? LineGrid.DefaultControlReference;
                Value = value;
            }

            string DebugDisplay()
            {
                return (ParentControlRef == null ? "NULL" : ParentControlRef == DefaultControlReference ? "EMPTY" : "CONTROL") + " | " + (Value == null ? "NULL" : Value.ToString());
            }
        }
    }
}
