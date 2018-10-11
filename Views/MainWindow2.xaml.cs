﻿using CarLeasingViewer.Controls.LeasingChartManagers;
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
    public partial class MainWindow2 : Window
    {
        public MainWindow2()
        {
            var vm = new LeasingViewViewModel(this);


            var curentYear = DateTime.Now.Year;
            var rMonth = Randomizer.GetRandomMonth(curentYear);
            MonthBusiness[] monthBuisnesses = null;

            if (App.SearchSettings.TestData)
            {
                App.SetAvailable(Month.GetMonthes(new DateTime(curentYear, 1, 1), new DateTime(curentYear, 12, 1)));
                monthBuisnesses = DataManager.GetDataset(App.AvailableMonthesAll.First(), App.AvailableMonthesAll.Last());
            }
            else
                monthBuisnesses = DataManager.GetDataset(rMonth, rMonth.Next(2));

            var set = new LeasingSet();
            set.Data = monthBuisnesses;

            InitializeComponent();
            DataContext = vm;

            //Set проставляем после инициализации, т.к. не явно заполняется контрол
            vm.LeasingSet = set;

            Loaded += MainWindow2_Loaded;
        }
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

        private void OnSearchSettingsChanged(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LeasingViewViewModel;

            if (vm == null)
                return;

            vm.Update();
        }

        private void TextBlock_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var vm = DataContext as LeasingViewViewModel;

            var model = (sender as FrameworkElement)?.DataContext as IIndexable;

            LeasingChart.HightlightManager.Hightlight(model == null ? -1 : model.Index);
        }

        private void ScrollViewer_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LeasingChart.HightlightManager.Clear();
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
    }
}
