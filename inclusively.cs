namespace CarLeasingViewer
{
    /// <summary>
    /// Флаг границы операций (включительно [n .. k] или нет (n .. k) )
    /// </summary>
    public enum inclusively
    {
        /// <summary>
        /// Исключительно (n .. k) (по умочланию)
        /// </summary>
        None,
        /// <summary>
        /// Включительно слева [n .. k)
        /// </summary>
        Left,
        /// <summary>
        /// Включительно справа (n .. k]
        /// </summary>
        Right,
        /// <summary>
        /// Включитально с обеих сторон [n .. k]
        /// </summary>
        Both
    }
}
