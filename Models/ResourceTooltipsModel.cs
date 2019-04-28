namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Всплывающие подсказии для иконок Ресурса
    /// </summary>
    public class ResourceTooltipsModel
    {
        /// <summary>
        /// Всплывающая подсказка для иконки Страховки
        /// </summary>
        public string[] Insurance { get; set; }
        /// <summary>
        /// Всплывающая подсказка для иконки Ремонта
        /// </summary>
        public string[] Maintenance { get; set; }
    }
}
