namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Модель для плашки занятости авто
    /// </summary>
    public class LeasingBarModel : ViewModels.ViewModelBase, IIndexable
    {
        private Business pv_Leasing;
        /// <summary>
        /// Возвращает или задаёт информацию о Занятости
        /// </summary>
        public Business Leasing { get { return pv_Leasing; } set { if (pv_Leasing != value) { pv_Leasing = value; CalculateParams(); OnPropertyChanged(); } } }

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
        }

        void CalculateOffset(Business b)
        {
            if (b == null)
                DayOffset = 0d;

            var offsetLeft = 0d;
            var startDay = b.DateStart.Day;

            //если начало в текущем месяце
            if (b.MonthCount == 1 || b.DateStart.Month == b.CurrentMonth.Index)
            {
                if (startDay > 1)
                {
                    //т.к. дня нумеруются с единицы, то для первого дня отступ будет 0 дней, для второго - 1 и т.д.
                    var dayOffsetCount = startDay - 1;
                    offsetLeft = dayOffsetCount * (pv_DayColumnWidth) + (dayOffsetCount * 1); //1 - ширина границ у колонок
                }
            }

            DayOffset = offsetLeft;
        }

        void CalculateWidth(Business b)
        {
            var dayCount = 1; //прибавляем единичку, так как при сложении/вычитании теряем день

            #region Вычисляем количество дней

            //если машину взяли/вернули в течении 1 месяца
            if (b.MonthCount == 1)
                dayCount += (b.DateEnd - b.DateStart).Days;

            //если машина занята несколько месяцев
            else
            {
                var currentMonth = b.CurrentMonth.Index;

                //для месяца, в котором начали съём
                if (b.DateStart.Month == currentMonth)
                    //отсчитываем от конца начального месяца
                    dayCount += (b.CurrentMonth.DayCount - b.DateStart.Day);

                //для месяца в котором закончили съём
                else if (b.DateEnd.Month == currentMonth)
                    dayCount = b.DateEnd.Day; //индекс дня - количество дней от начала месяца

                //если период начинается и заканчивается за пределами текущего месяца
                else
                {
                    //берём первую дату месяца
                    var curentDate = b.CurrentMonth[1];
                    //если 'начало' < 'текущая дата' < 'конец'
                    dayCount = ((b.DateStart < curentDate) && (curentDate < b.DateEnd))
                        ? b.CurrentMonth.DayCount //берём количество дней в текущем месяце (закрашиваем всё)
                        : 0; //0 - хз чего ещё делать. В этом месяце занятости не было, хз как сюда попало
                }
            }

            if (dayCount < 0)
            {
                //m_loger.Log("Получен отрицательный период аренды. Значение сброшено в 0", MessageType.Debug
                //    , new LogParameter("Съёмщик", b.Title)
                //    , new LogParameter("Комментарий", b.Comment)
                //    , new LogParameter("Дата начала", b.DateStart.ToShortDateString())
                //    , new LogParameter("Дата окончания", b.DateEnd.ToShortDateString()));

                dayCount = 0;
            }

            #endregion

            Width = (pv_DayColumnWidth * dayCount) + dayCount; //прибавляем количество дней, т.к. ширина границ - 1
        }
    }
}
