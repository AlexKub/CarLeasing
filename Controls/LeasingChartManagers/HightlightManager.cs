﻿using System;
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
            var row = m_chart.RowManager[index];

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
        /// Снятие подсветки со строки
        /// </summary>
        /// <param name="row">Строка</param>
        void UnLightRow(Row row)
        {
            row.Hightlight(DefaultBrush);
            row.HightlightState = HightlightAction.None;
            m_hightlighted.Remove(row);
        }

        void HightlightRow(Row row, Brush brush)
        {
            row.Hightlight(brush);
            m_hightlighted.Add(row);
        }

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
