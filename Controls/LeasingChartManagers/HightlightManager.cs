using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using static CarLeasingViewer.Controls.LeasingChartManagers.RowManager;

namespace CarLeasingViewer.Controls.LeasingChartManagers
{
    /// <summary>
    /// Управление подсветкой строк
    /// </summary>
    public class HightlightManager : IDisposable
    {
        /// <summary>
        /// Ссылка на текущий график
        /// </summary>
        LeasingChart m_chart;
        List<Row> m_hightlighted = new List<Row>();

        /// <summary>
        /// Подсветка при наведении мышью
        /// </summary>
        public Brush MouseOverBrush { get; set; }
        /// <summary>
        /// Подсветка по умолчанию
        /// </summary>
        public Brush DefaultBrush { get; set; }
        /// <summary>
        /// Подсветка при выборе строки
        /// </summary>
        public Brush SelectedBrush { get; set; }

        /// <summary>
        /// Подсвеченные строки
        /// </summary>
        public IReadOnlyList<Row> HightlightedRows { get { return m_hightlighted; } }

        /// <summary>
        /// Подсветка строки
        /// </summary>
        /// <param name="index">Индекс строки</param>
        /// <param name="action">Действие по подсветке</param>
        public void Hightlight(int index, HightlightAction action)
        {
            var row = GetRow(index);

            switch (action)
            {
                case HightlightAction.Hightlight:
                    //снимаем подсветку с других подсвеченных строк
                    //при этом, оставляем подсветку на выбранных (Selected) строках
                    foreach (var r in HightlightedRows.Where(r => r.HightlightState == HightlightAction.Hightlight).ToList())
                        UnLightRow(r);

                    if (row != null)
                        if (row.HightlightState != HightlightAction.Select)
                        {
                            HightlightRow(row, MouseOverBrush);
                            row.HightlightState = action;
                        }
                    break;
                case HightlightAction.Select:
                    Clear();
                    if (row != null)
                    {
                        HightlightRow(row, SelectedBrush);
                        row.HightlightState = HightlightAction.Select;
                        row.Selected = true;
                        m_chart.RowManager.CallRowSelectionChanged(row);
                    }
                    break;
                case HightlightAction.None:
                default:
                    Clear();
                    if (row != null)
                        UnLightRow(row);
                    break;
            }
        }

        /// <summary>
        /// Выбрать строку
        /// </summary>
        /// <param name="index">Индекс строки</param>
        public void Select(int index)
        {
            if (index >= 0)
            {
                //выбираем строку
                Hightlight(index, HightlightAction.Select);
            }
            else
                Hightlight(index, HightlightAction.None);
        }

        /// <summary>
        /// Снимает выбор
        /// </summary>
        /// <param name="index"></param>
        public void UnSelect(int index)
        {
            Clear();

            var row = GetRow(index);

            if (!row.RowLayout.Hightlighted)
                //подсвечиваем как при наведении мышью
                Hightlight(index, HightlightAction.Hightlight);
        }

        /// <summary>
        /// Подсветка строки
        /// </summary>
        /// <param name="index">Индекс строки</param>
        public void Hightlight(int index)
        {
            if (index >= 0)
            {
                var row = GetRow(index);
                if (!(row.HightlightState == HightlightAction.Select))
                    Hightlight(index, HightlightAction.Hightlight);
            }
            else
                Clear();
        }

        /// <summary>
        /// Снять подсветку со строки
        /// </summary>
        /// <param name="index">Индекс строки</param>
        public void UnHightlight(int index)
        {
            var row = GetRow(index);

            if (row != null)
                if (row.HightlightState != HightlightAction.Select)
                    UnLightRow(row);
        }

        public void UnHightlightAll()
        {
            for (int i = 0; i < m_chart.RowManager.Count; i++)
            {
                UnHightlight(i);
            }
        }

        Row GetRow(int index)
        {
            return m_chart.RowManager[index];
        }

        /// <summary>
        /// Снятие подсветки со строки
        /// </summary>
        /// <param name="row">Строка</param>
        void UnLightRow(Row row)
        {
            row.Hightlight(DefaultBrush);
            var oldState = row.HightlightState;
            row.HightlightState = HightlightAction.None;
            row.Selected = false;
            m_hightlighted.Remove(row);

            if (oldState == HightlightAction.Select)
                m_chart.RowManager.CallRowSelectionChanged(row);
        }

        void HightlightRow(Row row, Brush brush)
        {
            row.Hightlight(brush);
            m_hightlighted.Add(row);
        }

        /// <summary>
        /// Снимает все подсветки со всех строк
        /// </summary>
        public void Clear()
        {
            foreach (var r in HightlightedRows.ToList())
                UnLightRow(r);
        }

        public void Dispose()
        {
            Clear();

            m_chart = null;
        }

        public HightlightManager(LeasingChart chart)
        {
            m_chart = chart;

            //color table http://www.flounder.com/csharp_color_table.htm
            MouseOverBrush = Brushes.SkyBlue;
            MouseOverBrush.Freeze();
            DefaultBrush = Brushes.Transparent;
            DefaultBrush.Freeze();
            SelectedBrush = Brushes.DodgerBlue;
            DefaultBrush.Freeze();
        }

        /// <summary>
        /// Действия подсветки
        /// </summary>
        public enum HightlightAction
        {
            /// <summary>
            /// Сброс подсветки
            /// </summary>
            None,
            /// <summary>
            /// Подсветка
            /// </summary>
            Hightlight,
            /// <summary>
            /// Выбор элемента
            /// </summary>
            Select

        }
    }
}
