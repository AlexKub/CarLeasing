using CarLeasingViewer.Models;

namespace CarLeasingViewer.Interfaces
{
    /// <summary>
    /// Данные для отрисовки полоски на Графике занятости
    /// </summary>
    public interface IDrawableBar
    {
        /// <summary>
        /// Индекс строки
        /// </summary>
        int RowIndex { get; set; }

        /// <summary>
        /// Отображаемый Период
        /// </summary>
        IPeriod Period { get; }

        /// <summary>
        /// Набор, к которому принадлежит элемент
        /// </summary>
        LeasingSet Set { get; }

        /// <summary>
        /// Текст на полоске
        /// </summary>
        string Text { get; }

        /// <summary>
        /// Строки для всплывающего окна
        /// </summary>
        string[] ToolTipRows { get; }

        /// <summary>
        /// Генерация копии
        /// </summary>
        /// <returns>Новая полная копия</returns>
        /// <remarks>Для генерации отсортированной коллекции</remarks>
        IDrawableBar Clone();
    }
}
