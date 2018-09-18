using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using CarLeasingViewer.Models;

namespace CarLeasingViewer.Controls
{
    /// <summary>
    /// Interaction logic for DynamicDefinitionsGrid.xaml
    /// </summary>
    public partial class DynamicDefinitionsGrid : Grid
    {
        public DynamicDefinitionsGrid()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty dp_OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(DynamicDefinitionsGrid), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(Orientation),
            PropertyChangedCallback = (s, e) => { (s as DynamicDefinitionsGrid).DrawLines(); }
        });
        public Orientation Orientation { get { return (Orientation)GetValue(dp_OrientationProperty); } set { SetValue(dp_OrientationProperty, value); } }

        public static readonly DependencyProperty dp_DefinitionSizeProperty = DependencyProperty.Register(nameof(DefinitionSize), typeof(GridLength), typeof(DynamicDefinitionsGrid), new FrameworkPropertyMetadata() { DefaultValue = default(GridLength) });
        public GridLength DefinitionSize { get { return (GridLength)GetValue(dp_DefinitionSizeProperty); } set { SetValue(dp_DefinitionSizeProperty, value); } }

        public static readonly DependencyProperty dp_LinesCountProperty = DependencyProperty.Register(nameof(LinesCount), typeof(int), typeof(DynamicDefinitionsGrid), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(int),
            PropertyChangedCallback = (s, e) =>
            {
                var _this = (s as DynamicDefinitionsGrid);
                _this.DrawLines();

                var lineGrid = _this.LineGrid;
                var lineHeight = _this.DefinitionSize.Value;
                lineGrid.Rows = _this.ColumnDefinitions.Select(cd => new LineGrid.GridLineValue<double>(lineGrid, lineHeight)).ToList();
            }
        });
        public int LinesCount { get { return (int)GetValue(dp_LinesCountProperty); } set { SetValue(dp_LinesCountProperty, value); } }

        public static DependencyProperty dp_Month = DependencyProperty.Register(nameof(Month), typeof(Month), typeof(DynamicDefinitionsGrid), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(Month),
            PropertyChangedCallback = (s, e) => (s as DynamicDefinitionsGrid).Month = e.NewValue as Month
        });
        public Month Month { get { return (Month)GetValue(dp_Month); } set { SetValue(dp_Month, value); } }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == DataContextProperty)
            {
                var collection = e.NewValue as System.Collections.IEnumerable;

                if (collection != null)
                {
                    var linesCount = LinesCount;
                    if (linesCount == 0)
                    {
                        foreach (var item in collection)
                            //если количество строк не задано, рисуем по количеству элементов
                            linesCount++;

                        LinesCount = linesCount;
                    }
                }
            }

            base.OnPropertyChanged(e);
        }

        void CreateRowOrColumn()
        {
            if (Orientation == Orientation.Horizontal)
                ColumnDefinitions.Add(new ColumnDefinition() { Width = DefinitionSize });
            else
                RowDefinitions.Add(new RowDefinition() { Height = DefinitionSize });
        }

        void DrawLines()
        {
            if (LinesCount == 0)
                return;

            for (int i = 0; i < LinesCount; i++)
            {
                CreateRowOrColumn();
            }
        }
    }
}
