﻿using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CarLeasingViewer.Controls
{
    /// <summary>
    /// Interaction logic for WeekDaysPanel.xaml
    /// </summary>
    public partial class WeekDaysPanel : UserControl
    {
        List<WeekDay> m_selectingDays = new List<WeekDay>();
        int m_selectingStartDay = 0;

        #region Dependency properties

        public static DependencyProperty dp_ColumnWidth = DependencyProperty.Register(nameof(ColumnWidth), typeof(double), typeof(WeekDaysPanel), new FrameworkPropertyMetadata() { DefaultValue = 20d });
        public double ColumnWidth { get { return (double)GetValue(dp_ColumnWidth); } set { SetValue(dp_ColumnWidth, value); } }

        public static DependencyProperty dp_DayNames = DependencyProperty.Register(nameof(DayNames), typeof(ObservableCollection<string>), typeof(WeekDaysPanel), new FrameworkPropertyMetadata() { DefaultValue = default(ObservableCollection<string>) });
        public ObservableCollection<string> DayNames { get { return (ObservableCollection<string>)GetValue(dp_DayNames); } set { SetValue(dp_DayNames, value); } }

        public static DependencyProperty dp_DayIndexes = DependencyProperty.Register(nameof(DayIndexes), typeof(IEnumerable<string>), typeof(WeekDaysPanel), new FrameworkPropertyMetadata() { DefaultValue = default(IEnumerable<string>) });
        public IEnumerable<string> DayIndexes { get { return (IEnumerable<string>)GetValue(dp_DayIndexes); } set { SetValue(dp_DayIndexes, value); } }

        public static DependencyProperty dp_Month = DependencyProperty.Register(nameof(Month), typeof(Month), typeof(WeekDaysPanel)
            , new FrameworkPropertyMetadata()
            {
                DefaultValue = default(Month)
                ,
                PropertyChangedCallback = (o, e) =>
                {
                    var sender = o as WeekDaysPanel;

                    if (sender == null)
                        return;

                    var month = e.NewValue as Models.Month;

                    if (month == null || month.IsEmpty)
                        return;

                    sender.DayNames = new ObservableCollection<string>(GetDays(month.GetFirstDayOfWeek(), month.DayCount));
                    sender.DayIndexes = GetDaysIndexes(month.DayCount);
                }
            });
        public Month Month { get { return (Month)GetValue(dp_Month); } set { SetValue(dp_Month, value); } }

        public static DependencyProperty dp_SelectedDaysIn = DependencyProperty.Register(nameof(SelectedDaysIn), typeof(IEnumerable<WeekDay>), typeof(WeekDaysPanel), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(IEnumerable<WeekDay>)
        ,
            PropertyChangedCallback = (s, e) => { (s as WeekDaysPanel).SelectDays(e.NewValue as IEnumerable<WeekDay>); }
        });
        public IEnumerable<WeekDay> SelectedDaysIn { get { return (IEnumerable<WeekDay>)GetValue(dp_SelectedDaysIn); } set { SetValue(dp_SelectedDaysIn, value); } }

        public static DependencyProperty dp_SelectedDaysOut = DependencyProperty.Register(nameof(SelectedDaysOut), typeof(IEnumerable<WeekDay>), typeof(WeekDaysPanel), new FrameworkPropertyMetadata() { DefaultValue = default(IEnumerable<WeekDay>) });
        public IEnumerable<WeekDay> SelectedDaysOut { get { return (IEnumerable<WeekDay>)GetValue(dp_SelectedDaysOut); } set { SetValue(dp_SelectedDaysOut, value); } }

        public static DependencyProperty dp_TitleSearch = DependencyProperty.Register(nameof(TitleSearch), typeof(string), typeof(WeekDaysPanel), new FrameworkPropertyMetadata()
        {
            DefaultValue = default(string)
        ,
            PropertyChangedCallback = (s, e) => { (s as WeekDaysPanel).SetEmptySelection(); }
        });
        public string TitleSearch { get { return (string)GetValue(dp_TitleSearch); } set { SetValue(dp_TitleSearch, value); } }

        public static DependencyProperty dp_ContextSearch = DependencyProperty.Register(nameof(ContextSearch), typeof(string), typeof(WeekDaysPanel), new FrameworkPropertyMetadata() { DefaultValue = default(string) });
        public string ContextSearch { get { return (string)GetValue(dp_ContextSearch); } set { SetValue(dp_ContextSearch, value); } }

        public static DependencyProperty dp_DaysPanelWidth = DependencyProperty.Register(nameof(DaysPanelWidth), typeof(double), typeof(WeekDaysPanel), new FrameworkPropertyMetadata() { DefaultValue = default(double) });
        public double DaysPanelWidth { get { return (double)GetValue(dp_DaysPanelWidth); } set { SetValue(dp_DaysPanelWidth, value); } }

        #endregion

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            DaysPanelWidth = DaysBorder.ActualWidth;
        }

        public event Action<WeekDay> DaySelected;

        public WeekDaysPanel()
        {
            InitializeComponent();
        }

        static IEnumerable<string> GetDaysIndexes(int dayCount)
        {
            List<string> indexes = new List<string>();

            for (int i = 1; i <= dayCount; i++)
                indexes.Add(i.ToString());

            return indexes;
        }

        static IEnumerable<string> GetDays(DayOfWeek day, int daysCount)
        {
            if (daysCount == 0)
                return Enumerable.Empty<string>();

            var dayI = (int)day;

            List<string> days = new List<string>();

            var dayNames = new string[7];
            bool namesFilled = false;
            int weekLength = 7;

            var weekCount = daysCount / 7; //количество недель (пачек по 7 дней) в месяце
            int lastWeekLength = daysCount % 7; //количество дней в неполной неделе
            weekCount = lastWeekLength == 0 ? weekCount : weekCount + 1; //в месяце 4 полных и 1 или 0 неполных недель

            for (int i = 0; i < weekCount; i++)
            {
                //для последней недели другое количество дней (31 - 3; 30 - 2; 28 - 0)
                //в месяце 4 полных и 1 или 0 неполных недель
                //если i == 4 - идёт пятая неделя
                weekLength = i == 4 ? lastWeekLength : 7;

                for (int j = 0; j < weekLength; j++)
                {
                    if (!namesFilled) //если имена дней ещё не заполнены
                    {
                        dayNames[j] = day.GetShortName(); //сохраняем имя для текущего дня

                        dayI++;

                        if (dayI > 6) //воскресение == 0
                            dayI = 0;

                        day = (DayOfWeek)dayI; //пишем следующий день для следующей итерации
                    }

                    days.Add(dayNames[j]); //сохраняем имя из буфера
                }

                namesFilled = true;   //после первого прохода имена считать заполненными
            }

            return days;
        }

        void SetEmptySelection()
        {
            var set = GetSet();
            if (set == null)
                return;

            set.SelectingContext.Reset();
            //set.ResetSorting();
        }

        private void Day_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var day = GetDay(sender);

            if (day == null)
                return;

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    var set = GetSet();

                    if (set == null)
                        return;

                    if (day.Selected)
                    {
                        if (day.IsUnSelecting)
                            set.SelectingContext.Remove(day);
                        else
                            set.SelectingContext.Append(day);
                    }

                    day.IsUnSelecting = false;
                    //ничего не делаем, если зажат Shift/Ctrl - идёт мультиселект
                    //bool multiselectKeyPressed = ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    //        || (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

                    //if (m_selectingStartDay == 0)
                    //{
                    //    HighLightDay(day, false, multiselectKeyPressed);
                    //
                    //    SetEmptySelection();
                    //}
                    //else
                    //{
                    //    DaySelected?.Invoke(day);
                    //
                    //    //if (m_selectingDays.Count > 0)
                    //    SelectedDaysOut = m_selectingDays.ToList();
                    //
                    //    if (IsMultiselecting())
                    //        set.SelectingContext.Select(m_selectingDays.Min(), m_selectingDays.Max());
                    //    //set.Sort(m_selectingDays.Min().Date, m_selectingDays.Max().Date);
                    //    else
                    //        set.SelectingContext.Select(day);
                    //    //set.Sort(day.Date);
                    //}
                    break;
                default:
                    break;
            }

            //todo : select
        }

        private void Day_MouseEnter(object sender, MouseEventArgs e)
        {
            var day = GetDay(sender);

            if (day == null)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var set = GetSet();

                if (set != null)
                {
                    set.SelectingContext.PreSelect(day);
                }
            }
            else
            {
                bool alreadyHightlighted = day.Selected || day.Hightlighted;

                if (!alreadyHightlighted)
                    day.Hightlighted = true;
            }
        }

        private void Day_MouseLeave(object sender, MouseEventArgs e)
        {
            var day = GetDay(sender);

            if (day == null)
                return;

            day.Hightlighted = false;
        }

        private void Day_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var day = GetDay(sender);

            if (day == null)
                return;

            if (e.ChangedButton == MouseButton.Left)
            {
                var selectingContext = GetSet()?.SelectingContext;

                if (selectingContext == null)
                    return;

                if (day.Selected)
                {
                    if (selectingContext.Count > 1)
                    {
                        if (!IsMultiSelectKeyPressed())
                        {
                            selectingContext.PreReset();
                        }

                        selectingContext.PreSelect(day);
                    }
                    else
                        day.IsUnSelecting = true;
                }
                else
                {
                    if (!IsMultiSelectKeyPressed())
                    {
                        selectingContext.PreReset();
                    }

                    selectingContext.PreSelect(day);
                }

                //bool isMultiSelecting = ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                //                            || (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                //
                //m_selectingStartDay = m_selectingStartDay == day.Index ? 0 : day.Index;
                //
                ////изменяем подсветку для Дня при клике
                ////  кликнули первый раз - подсветили
                ////  кликнули повторно - убрали
                ////if (!HasHightlighted(day))
                ////{
                //
                //
                //HighLightDay(day, true, isMultiSelecting);
                //
                //DaySelected?.Invoke(day);
                ////NameSearchText = string.Empty;
                //
                //Keyboard.ClearFocus();
                //
                ////}
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                //DownlightDays();

                SetEmptySelection();
            }
        }

        void HighLightDay(WeekDay day, bool hightlight, bool multiselect = false)
        {
            //day.Selected = hightlight;

            if (!multiselect)
                m_selectingDays.Clear();

            if (hightlight)
                m_selectingDays.Add(day);
            else if (multiselect)
                m_selectingDays.Remove(day);

            DownlightDays(day, multiselect);
        }

        WeekDay GetDay(object sender)
        {
            var tb = sender as TextBlock;

            if (tb == null)
                return null;

            return tb.DataContext as WeekDay;
        }

        LeasingSet GetSet()
        {
            return (DataContext as MonthHeaderModel)?.OwnerSet;
        }

        void DownlightDays(WeekDay day = null, bool multiselect = false)
        {
            if (day == null && !multiselect)
            {
                m_selectingDays.Clear();
                multiselect = true;
            }

            var month = Month;

            if (month != null)
            {
                //убираем подсветку для остальных дней

                bool daySelected = false;
                //проходемся по всем дням месяца
                foreach (var otherDay in month.Days)
                {
                    //если текущий день - один из тех, что выбирает сейчас пользователь
                    daySelected = multiselect ? m_selectingDays.Any(d => d.Index == otherDay.Index) : otherDay.Index == day.Index;
                    if (daySelected)
                        continue; //пропускаем этот день

                    else
                    {
                        //если день был выбран ранее
                        if (otherDay.Selected)
                        {
                            //снимаем подсветку
                            //otherDay.Selected = false;
                            m_selectingDays.Remove(otherDay);
                        }
                    }
                }

            }
        }

        bool HasHightlighted(WeekDay day)
        {
            if (day == null)
                return false;

            return day.Selected || day.Hightlighted;
        }

        bool IsMultiselecting() { return m_selectingDays.Count > 1; }
        bool IsMultiSelectKeyPressed()
        {
            return ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) || ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);
        }

        void SelectDays(IEnumerable<WeekDay> days)
        {
            if (days == null)
                return;

            //SetEmptySelection();
            m_selectingDays.Clear();

            if (days.Count() == 0)
                DownlightDays();
            else
            {
                m_selectingDays.AddRange(days);

                foreach (var day in days)
                    HighLightDay(day, true, true);
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == DataContextProperty)
            {
                var context = DataContext as MonthHeaderModel;

                if (context != null)
                    Month = context.Month;
            }
        }


    }
}
