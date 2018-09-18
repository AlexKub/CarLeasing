namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Модель для Вкладки TabItem
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public abstract class TabItemModel : ViewModels.ViewModelBase
    {
        private string pv_Title;
        /// <summary>
        /// Возвращает или задаёт Заголовок вкладки
        /// </summary>
        public string Title { get { return pv_Title; } set { if (pv_Title != value) { pv_Title = value; OnPropertyChanged(); } } }

        public TabItemModel(string title)
        {
            Title = title;
        }
        public TabItemModel() : this("DEFAULT TITLE")
        {

        }

        string DebugDisplay()
        {
            return string.IsNullOrEmpty(Title) ? "NO TITLE" : Title;
        }
    }
}
