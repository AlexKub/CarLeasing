namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Модель для плашки занятости авто
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public partial class LeasingElementModel : ViewModels.ViewModelBase, IIndexable
    {
        /// <summary>
        /// Уникальный ID элемента в контексте Canvas'а
        /// </summary>
        public string ElementID { get; internal set; }

        private MonthHeaderModel pv_Month;
        /// <summary>
        /// Возвращает или задаёт Модель месяцА, с которым связана текущая Аренда
        /// </summary>
        public MonthHeaderModel Month { get { return pv_Month; } set { if (pv_Month != value) { pv_Month = value; OnPropertyChanged(); } } }

        private MonthHeaderModel[] m_Monthes;
        /// <summary>
        /// Возвращает или задаёт Месяца, к которым принадлежит текущая аренда
        /// </summary>
        public MonthHeaderModel[] Monthes { get { return m_Monthes; } set { m_Monthes = value; OnPropertyChanged(); } }

        private Business pv_Leasing;
        /// <summary>
        /// Возвращает или задаёт информацию о Занятости
        /// </summary>
        public Business Leasing { get { return pv_Leasing; } set { if (pv_Leasing != value) { pv_Leasing = value; CalculateParams(); OnPropertyChanged(); } } }

        private int m_DaysCount;
        /// <summary>
        /// Возвращает или задаёт Количество дней в аренде
        /// </summary>
        public int DaysCount { get { return m_DaysCount; } set { m_DaysCount = value; OnPropertyChanged(); } }

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

        #region IIndexable

        int IIndexable.Index { get => pv_RowIndex; set => pv_RowIndex = value; }

        #endregion

        void CalculateParams()
        {
            CalculateOffset(pv_Leasing);
            CalculateWidth(pv_Leasing);

            if (Leasing != null)
                DaysCount = (Leasing.DateEnd - Leasing.DateStart).Days + 1;
            else
                DaysCount = 0;
        }

        void CalculateOffset(Business b)
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
            else if(b.CurrentMonth == null || b.CurrentMonth != startMonth)
            {
                DayOffset = 0d;
                return;
            }

            dayCount += b.DateStart.Day - 1;

            //смещение слева в точках
            DayOffset = dayCount * pv_DayColumnWidth + (dayCount * 1);
        }

        void CalculateWidth(Business b)
        {
            var dayCount = 1; //прибавляем единичку, так как при сложении/вычитании теряем день

            #region Вычисляем количество дней

            //если машину взяли/вернули в течении 1 месяца
            if (b.MonthCount == 1)
            {
                dayCount += (b.DateEnd - b.DateStart).Days;
            }
            //если машина взята в аренду на несколько месяцев
            else
            {
                var startDate = b.DateStart;
                var endDate = b.DateEnd;

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

            Width = (pv_DayColumnWidth * dayCount) + dayCount; //прибавляем количество дней, т.к. ширина границ - 1
        }

        string DebugDisplay()
        {
            return (Month == null || Month.Month == null ? "NO MONTH" : (Month.Month.Name + " " + Month.Month.Year.ToString()))
                + " | " + CarName == null ? "NO CAR" : CarName;
        }
    }
}
