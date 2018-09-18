namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Модель для заказов по одному месяцу
    /// </summary>
    public class OneMonthItem : TabItemModel
    {
        #region 

        private MonthBusiness pv_MonthLeasing;

        /// <summary>
        /// Возвращает или задаёт
        /// </summary>
        public MonthBusiness MonthLeasing { get { return pv_MonthLeasing; } set { if (pv_MonthLeasing != value) { pv_MonthLeasing = value; OnPropertyChanged(); } } }


        #endregion

        public OneMonthItem(string title) : base(title)
        {
        }
    }
}
