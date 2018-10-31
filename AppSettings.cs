namespace CarLeasingViewer
{
    /// <summary>
    /// Стили приложения, используемые как в коде, так и в xaml
    /// </summary>
    public static class AppStyles
    {
        /// <summary>
        /// Возвращает или задаёт ширину колонки в сетке
        /// </summary>
        public static double ColumnWidth { get { return 20d; } }

        /// <summary>
        /// Сумма ширины вертикальной полоски и колонки
        /// </summary>
        public static double TotalColumnWidth { get { return GridLineWidth + ColumnWidth; } }

        /// <summary>
        /// Возращает ширину горизонтальных полосок на сетке
        /// </summary>
        public static double GridLineWidth { get { return 2d; } }

        /// <summary>
        /// Возращает отрицательную ширину горизонтальных полосок на сетке (для xaml margin's)
        /// </summary>
        public static double GridLineWidthNegative { get { return GridLineWidth * -1d; } }
    }
}
