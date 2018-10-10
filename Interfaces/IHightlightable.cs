using System.Windows.Media;

namespace CarLeasingViewer.Interfaces
{
    /// <summary>
    /// Подсвечиваемые контролы
    /// </summary>
    public interface IHightlightable
    {
        /// <summary>
        /// Кисть подсветки
        /// </summary>
        Brush HightlightBrush { get; set; }

        /// <summary>
        /// Флаг наличия подсветки
        /// </summary>
        bool Hightlighted { get; set; }
    }
}
