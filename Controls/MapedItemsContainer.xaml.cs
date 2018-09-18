using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CarLeasingViewer.Controls
{
    /// <summary>
    /// Interaction logic for MyViewModel.xaml
    /// </summary>
    public partial class MapedItemsContainer : ListBox
    {
        public static DependencyProperty dp_RowHeights = DependencyProperty.Register(nameof(RowHeights), typeof(IList<LineGrid.GridLineValue<double>>), typeof(MapedItemsContainer), new FrameworkPropertyMetadata() { DefaultValue = default(IEnumerable<LineGrid.GridLineValue<double>>) });
        public IList<LineGrid.GridLineValue<double>> RowHeights { get { return (IList<LineGrid.GridLineValue<double>>)GetValue(dp_RowHeights); } set { SetValue(dp_RowHeights, value); } }

        public static DependencyProperty dp_RightColumnWidth = DependencyProperty.Register(nameof(RightColumnWidth), typeof(double), typeof(MapedItemsContainer), new FrameworkPropertyMetadata() { DefaultValue = default(double) });
        public double RightColumnWidth { get { return (double)GetValue(dp_RightColumnWidth); } set { SetValue(dp_RightColumnWidth, value); } }

        public MapedItemsContainer()
        {
            InitializeComponent();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            ReDrawGrid();
        }

        void ReDrawGrid()
        {
            if (Items != null && Items.Count > 0)
            {
                var heights = new List<LineGrid.GridLineValue<double>>();

                for (int i = 0; i < Items.Count; i++)
                {
                    //получаем расчитанные ListBoxItem's
                    var panel = ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;

                    if (panel != null)
                        //сохраняем расчитанные высоты
                        heights.Add(new LineGrid.GridLineValue<double>(null, panel.ActualHeight));
                }

                //перерисовка сетки, при изменении набора высот строк
                RowHeights = heights;
            }
        }
    }
}
