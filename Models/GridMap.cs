using System.Collections.Generic;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Модель с координатами для сетки
    /// </summary>
    public class GridMap : ViewModels.ViewModelBase
    {
        private Dictionary<int, double> _RowHeights;
        /// <summary>
        /// Возвращает или задаёт Высоты строк
        /// </summary>
        public Dictionary<int, double> RowHeights { get { return _RowHeights; } set { _RowHeights = value; OnPropertyChanged(); } }

        private double _ColumnWidth;
        /// <summary>
        /// Возвращает или задаёт Ширину колонок
        /// </summary>
        public double ColumnWidth { get { return _ColumnWidth; } set { _ColumnWidth = value; OnPropertyChanged(); } }
    }
}
