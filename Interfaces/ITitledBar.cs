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
    }
}
