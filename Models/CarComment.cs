using System;

namespace CarLeasingViewer.Models
{
    public class CarComment : ViewModels.ViewModelBase, IIndexable
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
    }
}
