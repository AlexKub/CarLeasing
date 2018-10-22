using System.Windows.Media;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Модель данных для Комментария машины
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class CarCommentModel : ViewModels.ViewModelBase, IIndexable, Interfaces.IHightlightable
    {
        private int pv_RowIndex;
        /// <summary>
        /// Возвращает или задаёт
        /// </summary>
        public int RowIndex { get { return pv_RowIndex; } set { if (pv_RowIndex != value) { pv_RowIndex = value; OnPropertyChanged(); } } }

        private string pv_Comment;
        /// <summary>
        /// Возвращает или задаёт Комментарий
        /// </summary>
        public string Comment { get { return pv_Comment; } set { if (pv_Comment != value) { pv_Comment = value; OnPropertyChanged(); } } }

        #region IIndexable

        int IIndexable.Index { get => pv_RowIndex; set => pv_RowIndex = value; }

        #endregion

        #region IHightlightable

        private Brush pv_HightlightBrush;
        /// <summary>
        /// Возвращает или задаёт кисть подсветки
        /// </summary>
        public Brush HightlightBrush { get { return pv_HightlightBrush; } set { if (pv_HightlightBrush != value) { pv_HightlightBrush = value; OnPropertyChanged(); } } }

        /// <summary>
        /// Флаг подсветки
        /// </summary>
        public bool Hightlighted { get; set; }

        #endregion

        string DebugDisplay()
        {
            return RowIndex.ToString() + " | " + (string.IsNullOrEmpty(Comment) ? "NO COMMENT" : Comment);
        }

        /// <summary>
        /// Получение нового экземпляра с теми же значениями свойств, кроме RowIndex
        /// </summary>
        /// <returns>Возвращает новый экземпляр с теми же значениями свойств, кроме RowIndex</returns>
        public CarCommentModel Clone()
        {
            var newInstance = new CarCommentModel();
            newInstance.pv_Comment = pv_Comment;
            newInstance.pv_HightlightBrush = pv_HightlightBrush;

            return newInstance;
        }
    }
}
