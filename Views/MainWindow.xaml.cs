using CarLeasingViewer.Controls.LeasingChartManagers;
using CarLeasingViewer.Models;
using CarLeasingViewer.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace CarLeasingViewer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow2.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LeasingViewViewModel m_viewModel;

        /// <summary>
        /// Костыль для отключения автообновления при тесте
        /// </summary>
        bool m_firstLoad = false;

        public MainWindow()
        {
            m_viewModel = new LeasingViewViewModel();

            var curentYear = DateTime.Now.Year;
            var rMonth = Randomizer.GetRandomMonth(curentYear);
            MonthBusiness[] monthBuisnesses = null;

            if (App.TestMode && App.SearchSettings.TestData)
            {
                App.SetAvailable(Month.GetMonthes(new DateTime(curentYear, 1, 1), new DateTime(curentYear, 12, 1)));
                monthBuisnesses = DataManager.GetDataset(App.AvailableMonthesAll.First(), App.AvailableMonthesAll.Last());
            }
            else
            {
                App.SetAvailable(DB_Manager.Default.GetAvailableMonthes());
                
                App.SetRegions(DB_Manager.Default.GetRegions());

                var first = Month.Current.Previos();
                var last = Month.Current.Next();
                if (App.AvailableMonthesAll.Count() > 0)
                {
                    if (App.AvailableMonthesAll.Last() < last)
                    {
                        last = App.AvailableMonthesAll.Last();
                        first = last.Previos().Previos();
                    }
                }

                App.SetCars(DB_Manager.Default.GetAllCars(first, last));
                monthBuisnesses = DataManager.GetDataset(first, last);
            }

            var set = new LeasingSet();
            set.Data = monthBuisnesses;

            InitializeComponent();
            set.Chart = LeasingChart;

            Subscribe(true);

            DataContext = m_viewModel;

            //Set проставляем после инициализации, т.к. не явно заполняется контрол
            m_viewModel.LeasingSet = set;

            Loaded += MainWindow2_Loaded;
        }

        private void MainWindow2_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow2_Loaded;

            LeasingChart.VisibleArea.ChartHeight = LeasingChart.ActualHeight;
            LeasingChart.VisibleArea.ChartWith = LeasingChart.ActualWidth;

            //перерисовка графика теперь в OnActivated
            //LeasingChart.Draw();
        }

        #region Общий скролл для каждой из областей

        private void LeasingScroll_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            MonthesScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);

            CarColumnScroll.ScrollToVerticalOffset(e.VerticalOffset);
            CommentsColumnScroll.ScrollToVerticalOffset(e.VerticalOffset);

            LeasingChart.VisibleArea.HorisontalScrollOffset = e.HorizontalOffset;
        }

        private void CommentsColumnScroll_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            LeasingScroll.ScrollToVerticalOffset(e.VerticalOffset);
            LeasingChart.VisibleArea.VerticalScrollOffset = e.VerticalOffset;

            CarColumnScroll.ScrollToVerticalOffset(e.VerticalOffset);
        }

        private void CarColumnScroll_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            LeasingScroll.ScrollToVerticalOffset(e.VerticalOffset);
            LeasingChart.VisibleArea.VerticalScrollOffset = e.VerticalOffset;
            CommentsColumnScroll.ScrollToVerticalOffset(e.VerticalOffset);
        }

        #endregion

        protected override void OnClosing(CancelEventArgs e)
        {
            var vm = DataContext as LeasingViewViewModel;

            Subscribe(false);

            if (vm != null)
                vm.Dispose();

            base.OnClosing(e);
        }

        private void OnSearchSettingsChanged(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LeasingViewViewModel;

            if (vm == null)
                return;

            vm.Update();
        }

        #region Подсветка строк при наведении мышью

        private void TextBlock_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var vm = DataContext as LeasingViewViewModel;

            var model = (sender as FrameworkElement)?.DataContext as IIndexable;

            LeasingChart.HightlightManager.Hightlight(model == null ? -1 : model.Index);
        }

        private void ScrollViewer_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LeasingChart.HightlightManager.UnHightlightAll();
        }

        private void TextBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var vm = DataContext as LeasingViewViewModel;

            var model = (sender as FrameworkElement)?.DataContext as IIndexable;

            LeasingChart.HightlightManager.Select(model == null ? -1 : model.Index);
        }

        private void TextBlock_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var vm = DataContext as LeasingViewViewModel;

            var model = (sender as FrameworkElement)?.DataContext as IIndexable;

            LeasingChart.HightlightManager.UnSelect(model == null ? -1 : model.Index);
        }

        private void Grid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //снимаем подсветку при наведении при выходе мыши за границы контрола
            LeasingChart.HightlightManager.UnHightlightAll();
        }

        #endregion

        private void LeasingScroll_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LeasingChart.VisibleArea.ChartHeight = LeasingScroll.ActualHeight;
            LeasingChart.VisibleArea.ChartWith = LeasingScroll.ActualWidth;
            LeasingChart.ReDrawGrid();
        }

        void Subscribe(bool subscribe)
        {
            if (subscribe)
            {
                m_viewModel.SetChanged += M_viewModel_SetChanged;
                LeasingChart.RowSelectionChanged += LeasingChart_RowSelectionChanged;
                Activated += MainWindow2_Activated;
            }
            else
            {
                m_viewModel.SetChanged -= M_viewModel_SetChanged;
                LeasingChart.RowSelectionChanged -= LeasingChart_RowSelectionChanged;
                Activated -= MainWindow2_Activated;
            }
        }

        private void M_viewModel_SetChanged(LeasingSetEventArgs e)
        {
            if (e.New != null)
            {
                e.New.Chart = LeasingChart;

                LeasingChart.LeasingSet = e.New;

                LeasingChart.Draw();
            }
        }

        private void MainWindow2_Activated(object sender, EventArgs e)
        {
            //отключаем в тест_моде, а то дебажить невозможно
            //каждый раз при переходе в Студию окно перерисовывается
            if (App.TestMode)
            {
                if (!m_firstLoad)
                    //для первой загрузки включаем, т.к. необходимо проверить отрисовку при свёрнутом окне
                    m_firstLoad = true;
                else
                    return;
            }

            m_viewModel.Update();

            //перерисовка статистики при разворачивании
            LeasingChart.Draw();
        }

        private void LeasingChart_RowSelectionChanged(RowManager.Row row)
        {
            var vm = DataContext as LeasingViewViewModel;

            if (vm != null)
            {
                var s = new StatisticModel();

                if (row.Selected)
                    //отображение статистики по выбранной строке
                    s.Load(row, vm.LeasingSet);
                else
                    //отображение статистики по текущему набору
                    s.Load(vm.LeasingSet);

                vm.Statistic = s;
            }
        }

    }
}
