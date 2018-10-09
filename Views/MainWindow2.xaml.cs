using CarLeasingViewer.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace CarLeasingViewer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow2.xaml
    /// </summary>
    public partial class MainWindow2 : Window
    {
        public MainWindow2(LeasingViewViewModel vm)
        {
            vm.Window = this;
            DataContext = vm;
            InitializeComponent();

            Loaded += MainWindow2_Loaded;
        }

        private void MainWindow2_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow2_Loaded;

            LeasingChart.Draw();
        }

        private void LeasingScroll_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            MonthesScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);

            CarColumnScroll.ScrollToVerticalOffset(e.VerticalOffset);
            CommentsColumnScroll.ScrollToVerticalOffset(e.VerticalOffset);
        }

        private void CommentsColumnScroll_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            LeasingScroll.ScrollToVerticalOffset(e.VerticalOffset);
            CarColumnScroll.ScrollToVerticalOffset(e.VerticalOffset);
        }

        private void CarColumnScroll_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            LeasingScroll.ScrollToVerticalOffset(e.VerticalOffset);
            CommentsColumnScroll.ScrollToVerticalOffset(e.VerticalOffset);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var vm = DataContext as LeasingViewViewModel;

            if (vm != null)
                vm.Dispose();

            base.OnClosing(e);
        }

        
    }
}
