using CarLeasingViewer.Models;
using RTCManifestGenerator.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CarLeasingViewer.Controls
{
    /// <summary>
    /// Interaction logic for PeriodSelector.xaml
    /// </summary>
    public partial class PeriodSelector : UserControl
    {
        static int m_curentYear = DateTime.Now.Year;
        static IEnumerable<Month> m_curentYearMonthes = Month.GetMonthes(m_curentYear);

        #region Dependency properties

        public static DependencyProperty dp_ShowAction = DependencyProperty.Register(nameof(ShowAction), typeof(ActionCommand), typeof(PeriodSelector), new FrameworkPropertyMetadata() { DefaultValue = default(ActionCommand) });
        public ActionCommand ShowAction { get { return (ActionCommand)GetValue(dp_ShowAction); } set { SetValue(dp_ShowAction, value); } }

        public static readonly DependencyProperty dp_FromMonthesProperty = DependencyProperty.Register(nameof(FromMonthes), typeof(IEnumerable<Month>), typeof(PeriodSelector), new FrameworkPropertyMetadata()
        {
            DefaultValue = m_curentYearMonthes,
            PropertyChangedCallback = (s, e) =>
            {
                var _this = s as PeriodSelector;
                _this.RefreshSelectedIndex(Period.From);
            }
        });
        public IEnumerable<Month> FromMonthes { get { return (IEnumerable<Month>)GetValue(dp_FromMonthesProperty); } set { SetValue(dp_FromMonthesProperty, value); } }

        public static readonly DependencyProperty dp_ToMonthesProperty = DependencyProperty.Register(nameof(ToMonthes), typeof(IEnumerable<Month>), typeof(PeriodSelector), new FrameworkPropertyMetadata()
        {
            DefaultValue = m_curentYearMonthes,
            PropertyChangedCallback = (s, e) =>
            {
                var _this = s as PeriodSelector;
                _this.RefreshSelectedIndex(Period.To);
            }
        });
        public IEnumerable<Month> ToMonthes { get { return (IEnumerable<Month>)GetValue(dp_ToMonthesProperty); } set { SetValue(dp_ToMonthesProperty, value); } }

        public static DependencyProperty dp_AvailableYears = DependencyProperty.Register(nameof(AvailableYears), typeof(IEnumerable<int>), typeof(PeriodSelector), new FrameworkPropertyMetadata()
        {
            DefaultValue = new int[] { m_curentYear },
            PropertyChangedCallback = (s, e) =>
            {
                var _this = s as PeriodSelector;
                _this.RefreshSelectedYear();
            }
        });
        public IEnumerable<int> AvailableYears { get { return (IEnumerable<int>)GetValue(dp_AvailableYears); } set { SetValue(dp_AvailableYears, value); } }

        public static readonly DependencyProperty dp_FromMonthIndexProperty = DependencyProperty.Register(nameof(FromMonthIndex), typeof(int), typeof(PeriodSelector), new FrameworkPropertyMetadata()
        {
            DefaultValue = -1,
            PropertyChangedCallback = (s, e) =>
            {
                var _this = s as PeriodSelector;
                _this.RefreshSelectedMonth(Period.From);
            }
        });
        public int FromMonthIndex { get { return (int)GetValue(dp_FromMonthIndexProperty); } set { SetValue(dp_FromMonthIndexProperty, value); } }

        public static readonly DependencyProperty dp_ToMonthIndexProperty = DependencyProperty.Register(nameof(ToMonthIndex), typeof(int), typeof(PeriodSelector), new FrameworkPropertyMetadata()
        {
            DefaultValue = -1,
            PropertyChangedCallback = (s, e) =>
            {
                var _this = s as PeriodSelector;
                _this.RefreshSelectedMonth(Period.To);
            }
        });
        public int ToMonthIndex { get { return (int)GetValue(dp_ToMonthIndexProperty); } set { SetValue(dp_ToMonthIndexProperty, value); } }

        public static readonly DependencyProperty dp_IncorrectPeriodProperty = DependencyProperty.Register(nameof(IncorrectPeriod), typeof(bool), typeof(PeriodSelector), new FrameworkPropertyMetadata() { DefaultValue = default(bool) });
        public bool IncorrectPeriod { get { return (bool)GetValue(dp_IncorrectPeriodProperty); } set { SetValue(dp_IncorrectPeriodProperty, value); } }

        public static readonly DependencyProperty dp_ErrorTextProperty = DependencyProperty.Register(nameof(ErrorText), typeof(string), typeof(PeriodSelector), new FrameworkPropertyMetadata() { DefaultValue = string.Empty });
        public string ErrorText { get { return (string)GetValue(dp_ErrorTextProperty); } set { SetValue(dp_ErrorTextProperty, value); } }

        public static readonly DependencyProperty dp_FromYearProperty = DependencyProperty.Register(nameof(FromYear), typeof(int), typeof(PeriodSelector), new FrameworkPropertyMetadata()
        {
            DefaultValue = m_curentYear,
            PropertyChangedCallback = (s, e) =>
            {
                var _this = s as PeriodSelector;
                _this.RefreshSelectedIndex(Period.From);
            }
        });
        public int FromYear { get { return (int)GetValue(dp_FromYearProperty); } set { SetValue(dp_FromYearProperty, value); } }

        public static readonly DependencyProperty dp_ToYearProperty = DependencyProperty.Register(nameof(ToYear), typeof(int), typeof(PeriodSelector), new FrameworkPropertyMetadata()
        {
            DefaultValue = m_curentYear,
            PropertyChangedCallback = (s, e) =>
            {
                var _this = s as PeriodSelector;
                _this.RefreshSelectedIndex(Period.To);
            }
        });
        public int ToYear { get { return (int)GetValue(dp_ToYearProperty); } set { SetValue(dp_ToYearProperty, value); } }

        public static readonly DependencyProperty dp_FromMonthProperty = DependencyProperty.Register(nameof(FromMonth), typeof(Month), typeof(PeriodSelector), new FrameworkPropertyMetadata()
        {
            DefaultValue = m_curentYearMonthes.FirstOrDefault(),
            PropertyChangedCallback = (s, e) =>
            {
                var _this = s as PeriodSelector;
                var month = e.NewValue as Month;

                if (month != null)
                {
                    if (_this.FromYear != month.Year)
                    {
                        _this.FromMonthes = DB_Manager.Default.GetAvailableMonthes(year: month.Year);

                        _this.RefreshSelectedIndex(Period.From);

                        _this.FromYear = month.Year;
                    }

                    _this.ValidatePeriod();
                }
            }
        });
        public Month FromMonth { get { return (Month)GetValue(dp_FromMonthProperty); } set { SetValue(dp_FromMonthProperty, value); } }

        public static readonly DependencyProperty dp_ToMonthProperty = DependencyProperty.Register(nameof(ToMonth), typeof(Month), typeof(PeriodSelector), new FrameworkPropertyMetadata()
        {
            DefaultValue = m_curentYearMonthes.FirstOrDefault(),
            PropertyChangedCallback = (s, e) =>
            {
                var _this = s as PeriodSelector;
                var month = e.NewValue as Month;

                if (month != null)
                {
                    if (_this.ToYear != month.Year)
                    {
                        _this.ToMonthes = DB_Manager.Default.GetAvailableMonthes(year: month.Year);

                        _this.RefreshSelectedIndex(Period.To);

                        _this.ToYear = month.Year;
                    }

                    _this.ValidatePeriod();
                }
            }
        });
        public Month ToMonth { get { return (Month)GetValue(dp_ToMonthProperty); } set { SetValue(dp_ToMonthProperty, value); } }

        #endregion

        public PeriodSelector()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Проверка нормальности выбранного периода
        /// </summary>
        void ValidatePeriod()
        {
            if (FromMonth > ToMonth)
            {
                IncorrectPeriod = true;
                ErrorText = "Месяц начала позднее месяца окончания";
            }
            else
            {
                IncorrectPeriod = false;
                ErrorText = string.Empty;
            }
        }

        /// <summary>
        /// Перевыбор индекса у месяца при смене года (в новом году каких-то месяцев в БД может не быть, следовательно коллекция и индексы изменятся)
        /// </summary>
        void RefreshSelectedIndex(Period p)
        {
            if (FromMonth == null) //если месяц не проставлен по каким-то причинам
            {
                if (FromMonthIndex >= 0)
                    FromMonthIndex = -1; //ставим отрицательное значение
            }
            else
            {
                //проверка, что индекс не равен текущему 
                //(предотвращение рекурсии, т.к. при изменении индекса вызывается соответствующее изменение месяца)
                //и простановка индекса ткеущего выбранного месяца
                if (p == Period.From)
                {
                    var fromMonthIndex = FromMonth.Index;
                    var newIndex = FromMonthes.IndexOf(FromMonth, (m) => m.Index == fromMonthIndex);

                    if (newIndex != FromMonthIndex)
                        FromMonthIndex = newIndex;
                }
                else if (p == Period.To)
                {

                    var toMonthIndex = ToMonth.Index;
                    var newIndex = ToMonthes.IndexOf(ToMonth, (m) => m.Index == toMonthIndex);

                    if (newIndex != ToMonthIndex)
                        ToMonthIndex = newIndex;
                }
            }
        }

        /// <summary>
        /// Перевыбор месяца при смене выбранного индекса
        /// </summary>
        void RefreshSelectedMonth(Period p)
        {
            if (p == Period.From)
            {
                if (FromMonthIndex >= 0)
                {
                    var fromMonthIndex = FromMonthIndex;
                    var newMonth = FromMonthes.ElementAt(fromMonthIndex);

                    if (!newMonth.Equals(FromMonth))
                        FromMonth = newMonth;
                }
            }
            else if (p == Period.To)
            {
                if (ToMonthIndex >= 0)
                {
                    var toMonthIndex = ToMonthIndex;
                    var newMonth = ToMonthes.ElementAt(toMonthIndex);

                    if (!newMonth.Equals(ToMonth))
                        ToMonth = newMonth;
                }
            }
        }

        void RefreshSelectedYear()
        {
            if (FromMonth != null)
            {
                if (FromYear != FromMonth.Year)
                    FromYear = FromMonth.Year;
            }

            if (ToMonth != null)
            {
                if (ToYear != ToMonth.Year)
                    ToYear = ToMonth.Year;
            }
        }

        private enum Period
        {
            From,
            To
        }
    }
}
