namespace CarLeasingViewer
{
    /// <summary>
    /// Элементы, влияющие на размер строки/колонки в динмически сгенерированных Grid'ах
    /// </summary>
    public interface IGridLengthManager
    {
        /// <summary>
        /// Высота/ширина текущей строки/колонки, к которой принадлежит контрол
        /// </summary>
        System.Windows.GridLength GridItemSize { get; set; }
    }
}
