namespace CarLeasingViewer.Interfaces
{
    /// <summary>
    /// Элемент на графике с Описанием
    /// </summary>
    public interface ITitledBar : IDrawableBar
    {
        /// <summary>
        /// Описание
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Количество видимых дней на Графике
        /// </summary>
        int VisibleDaysCount { get; }
    }
}
