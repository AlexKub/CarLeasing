namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Модель для TabItem с Регионом
    /// </summary>
    public class RegionTabItemModel : PeriodTabItem
    {
        private Region pv_Region;
        /// <summary>
        /// Возвращает или задаёт Регион
        /// </summary>
        public Region Region { get { return pv_Region; } set { if (pv_Region != value) { pv_Region = value; OnPropertyChanged(); } } }


        public RegionTabItemModel(string title) : base(title)
        {
        }
    }
}
