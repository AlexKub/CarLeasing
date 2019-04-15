using CarLeasingViewer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Модель для плашки занятости авто
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public partial class LeasingBarModel : ViewModels.ViewModelBase, IIndexable, ITitledBar
    {
#if Test
        bool m_calculate = false;
#endif

        /// <summary>
        /// Уникальный ID элемента в контексте Canvas'а
        /// </summary>
        public string ElementID { get; internal set; }

        /// <summary>
        /// Набор, к которому принадлежит элемент
        /// </summary>
        public LeasingSet Set { get; private set; }

        #region Notify properties

        private MonthHeaderModel pv_Month;
        /// <summary>
        /// Возвращает или задаёт Модель месяцА, с которым связана текущая Аренда
        /// </summary>
        public MonthHeaderModel Month { get { return pv_Month; } set { if (pv_Month != value) { pv_Month = value; OnPropertyChanged(); } } }

        private MonthHeaderModel[] pv_Monthes;
        /// <summary>
        /// Возвращает или задаёт Месяца, к которым принадлежит текущая аренда
        /// </summary>
        public MonthHeaderModel[] Monthes { get { return pv_Monthes; } set { pv_Monthes = value; OnPropertyChanged(); } }

        private Leasing pv_Leasing;
        /// <summary>
        /// Возвращает или задаёт информацию о Занятости
        /// </summary>
        public Leasing Leasing
        {
            get { return pv_Leasing; }
            set
            {
                if (pv_Leasing != value)
                {
                    pv_Leasing = value;

#if Test
                    if (m_calculate)
                        CalculateParams();
#else
                    CalculateParams();
#endif
                    VisibleDaysCount = (int)value.DayCount;

                    OnPropertyChanged();
                }
            }
        }

        private int pv_RowIndex;
        /// <summary>
        /// Возвращает или задаёт индекс строки Grid'а, к которому будет приставлен контрол
        /// </summary>
        public int RowIndex { get { return pv_RowIndex; } set { if (pv_RowIndex != value) { pv_RowIndex = value; OnPropertyChanged(); } } }

        private double pv_DayColumnWidth;
        /// <summary>
        /// Возвращает или задаёт Ширину колонки для одного дня
        /// </summary>
        public double DayColumnWidth
        {
            get { return pv_DayColumnWidth; }
            set
            {
                if (pv_DayColumnWidth != value)
                {
                    pv_DayColumnWidth = value; OnPropertyChanged();
                    CalculateParams();
                }
            }
        }

        private double pv_DayOffset;
        /// <summary>
        /// Возвращает или задаёт Отступ в днях от начала месяца
        /// </summary>
        public double DayOffset { get { return pv_DayOffset; } set { if (pv_DayOffset != value) { pv_DayOffset = value; OnPropertyChanged(); } } }

        private double pv_Width;
        /// <summary>
        /// Возвращает или задаёт Ширину полоски
        /// </summary>
        public double Width { get { return pv_Width; } set { if (pv_Width != value) { pv_Width = value; OnPropertyChanged(); } } }

        private string pv_CarName;
        /// <summary>
        /// Возвращает или задаёт
        /// </summary>
        public string CarName { get { return pv_CarName; } set { if (pv_CarName != value) { pv_CarName = value; OnPropertyChanged(); } } }

        #endregion

        /// <summary>
        /// Вовзращает видимое количество дней
        /// </summary>
        public int VisibleDaysCount { get; private set; }

#if Test

        public LeasingElementModel()
        {
            //не подсчитывать параметры для фейков, если не указано явно
            m_calculate = false;
        }

#endif


        public LeasingBarModel(LeasingSet set)
        {
            Set = set;
        }

        /// <summary>
        /// Сторнирование (усечение) аренды
        /// </summary>
        /// <param name="storno">Период сторнирования</param>
        public void Storning(IPeriod storno)
        {
            if (storno == null)
                return;
            if (pv_Leasing == null)
                return;

            //получаем пересечение периодов
            var storned = storno.CrossPeriod(pv_Leasing);

            if (storned.IsZero)
                return;

            //если сторнирующий начался раньше или одновременно с текущим
            if (storned.DayIndexStart >= pv_Leasing.DayIndexStart)
                //меняем дату начала
                Leasing.DateStart = storned.DateEnd;

            //если сторнирующий начался после текущего
            else
                //меняем дату окончания
                Leasing.DateEnd = storned.DateStart;

            var stornedCount = Leasing.DayCount - storno.DayCount;


            if (stornedCount <= 0m)
            {
                stornedCount = 0;
                VisibleDaysCount = 0;
            }
            else
            {
                var visibleCount = Set.CrossDaysCount(Leasing);
                VisibleDaysCount = (int)(visibleCount < stornedCount ? visibleCount : stornedCount);
            }
        }

        #region IIndexable

        int IIndexable.Index { get => pv_RowIndex; set => pv_RowIndex = value; }

        #endregion

        #region IDrawableBar

        bool IDrawableBar.Visible => VisibleDaysCount > 0;

        IPeriod IDrawableBar.Period => pv_Leasing;

        LeasingSet IDrawableBar.Set => Set;

        public int ZIndex => 0;

        string[] IDrawableBar.ToolTipRows
        {
            get
            {
                //формирование строчек всплывающей подсказски
                //для Аренды
                var rows = new List<string>(4);

                rows.Add(pv_Leasing?.Title ?? "NULL");
                rows.Add(pv_CarName);
                rows.Add(GetDataSpan());

                var comment = pv_Leasing?.Comment;
                if (!string.IsNullOrEmpty(comment))
                    rows.Add(comment);

                return rows.ToArray();
            }
        }

        public string Title => Leasing?.Title ?? "NO_TITLE";

        public Controls.LeasingChartManagers.ChartBarType BarType => Controls.LeasingChartManagers.ChartBarType.Leasing;


        /// <summary>
        /// Получение строкового представления срока аренды
        /// </summary>
        /// <param name="model">Модель</param>
        /// <returns>Возвращает срок аренды</returns>
        string GetDataSpan()
        {
            //копипаста из BussinessDateConverter (старая версия)
            StringBuilder sb = new StringBuilder();
            //<действие> c XX по ХХ <месяц>
            string inline = "    ";
            sb.AppendLine("В аренде:");
            pv_Leasing.AppendTooltipRow(sb, inline);

            return sb.ToString();
        }

        /// <summary>
        /// Получение нового экземпляра с теми же значениями свойств, кроме RowIndex
        /// </summary>
        /// <returns>Возвращает новый экземпляр с теми же значениями свойств, кроме RowIndex</returns>
        public IDrawableBar Clone()
        {
            var newInstance = new LeasingBarModel(Set);
            newInstance.pv_CarName = pv_CarName;
            newInstance.pv_Width = pv_Width;
            newInstance.pv_Month = pv_Month;
            newInstance.pv_DayColumnWidth = pv_DayColumnWidth;
            newInstance.pv_DayOffset = pv_DayOffset;
            newInstance.pv_Leasing = pv_Leasing;
            newInstance.pv_Monthes = pv_Monthes;
            newInstance.VisibleDaysCount = VisibleDaysCount;

            return newInstance;
        }

        #endregion


        #region private


        void CalculateParams()
        {
            CalculateOffset(pv_Leasing);
            CalculateWidth(pv_Leasing);
        }

        void CalculateOffset(Leasing b)
        {
            if (b == null)
                DayOffset = 0d;

            var dayCount = 0;
            var startMonth = b.DateStart.GetMonth();

            //если начало в текущем месяце
            if (b.Monthes != null)
            {
                if (b.Monthes[0] > startMonth) //если съем начался ранее
                {
                    DayOffset = 0d;
                    return;
                }
                else
                {
                    if (b.Monthes[0] < startMonth) //если первый месяц выбранного периода начинается раньше
                    {
                        var prevMonth = b.Monthes[0];

                        //перебираем месяцы с лева на право
                        //пока не наткнёмся на начальный месяц съёма авто
                        do
                        {
                            dayCount += prevMonth.DayCount;
                            prevMonth = prevMonth.Next();
                        }
                        while (prevMonth != null && prevMonth != startMonth);
                    }
                }
            }
            //по каким-то причинам не заданы месяцы или дата начала ранее начального месяца
            else if (b.CurrentMonth == null || b.CurrentMonth != startMonth)
            {
                DayOffset = 0d;
                return;
            }

            dayCount += b.DateStart.Day - 1;

            //смещение слева в точках
            DayOffset = dayCount * pv_DayColumnWidth + (dayCount * 1);
        }

        void CalculateWidth(Leasing b)
        {
            var dayCount = 1; //прибавляем единичку, так как при сложении/вычитании теряем день

            #region Вычисляем количество дней

            //если машину взяли/вернули в течении 1 месяца
            if (b.MonthCount == 1)
            {
                dayCount += (b.DateEnd.Date - b.DateStart.Date).Days;
            }
            //если машина взята в аренду на несколько месяцев
            else
            {
                var startDate = b.DateStart.Date;
                var endDate = b.DateEnd.Date;

                //если съём начался за пределами первого месяца в выбранном периоде
                if (b.Monthes[0] > b.DateStart.GetMonth())
                    startDate = b.Monthes[0][1];

                //если съём заканчивается за последним месяцем в выбранном периоде
                if (b.Monthes[b.Monthes.Length - 1] < b.DateEnd.GetMonth())
                    endDate = b.Monthes[b.Monthes.Length - 1].LastDate;

                //получаем месяцы между датами (включительно)
                var monthes = Models.Month.GetMonthes(startDate, endDate);

                //суммируем дни в полученном периоде
                var lastIndex = monthes.Length - 1;
                for (int i = 0; i < monthes.Length; i++)
                {
                    if (i == 0)
                    {
                        if (startDate.Day == 1)
                            dayCount += monthes[i].DayCount;
                        else
                            dayCount += (monthes[i].DayCount - startDate.Day + 1);
                    }
                    else if (i == lastIndex)
                    {
                        dayCount += endDate.Day;
                    }
                    else
                        dayCount += monthes[i].DayCount;
                }
            }

            if (dayCount < 0)
            {
                dayCount = 0;
            }

            #endregion

            Width = (pv_DayColumnWidth * dayCount) + (dayCount * AppStyles.ColumnWidth); //плюс ширины границ
        }

        string DebugDisplay()
        {
            return (Month == null || Month.Month == null ? "NO MONTH" : (Month.Month.Name + " " + Month.Month.Year.ToString()))
                + " | " + CarName == null ? "NO CAR" : CarName;
        }

        #endregion
    }
}
