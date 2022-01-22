using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CarLeasingViewer.Controls.LeasingChartManagers
{
    public delegate void ColumnSelectedEvent(Column column);

    /// <summary>
    /// Поддержка абстракции строк на графике. Зависит от инициализации прочих менеджеров
    /// </summary>
    public class ColumnManager : IDisposable
    {
        LeasingChart m_chart;

        readonly Dictionary<int, Column> m_columns = new Dictionary<int, Column>();

        /// <summary>
        /// Получение строки по индексу
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Column this[int index] { get { return m_columns.ContainsKey(index) ? m_columns[index] : null; } }

        /// <summary>
        /// Строки
        /// </summary>
        public IEnumerable<Column> Columns { get { return m_columns.Values; } }

        /// <summary>
        /// Количество строк
        /// </summary>
        public int Count { get { return m_columns.Count; } }

        public event ColumnSelectedEvent ColumnSelectionChanged;

        public ColumnManager(LeasingChart chart)
        {
            m_chart = chart;

            Subscribe(true);
        }

        void Subscribe(bool subscribe)
        {
            if (m_chart == null)
                return;

            SubscribeLayoutManager(m_chart.ColumnLayoutDrawer, subscribe);

            if (subscribe)
            {
                m_chart.SetChanged += M_chart_SetChanged;
            }
            else
            {
                m_chart.SetChanged -= M_chart_SetChanged;
            }
        }

        /// <summary>
        /// Получение строки по координатам
        /// </summary>
        /// <param name="p">Искомая точка</param>
        /// <returns>Возвращает строку, к которой принадлежт точка или null</returns>
        public Column GetColumnByPoint(Point p)
        {
            var columnIndex = (int)(p.Y / (m_chart.RowHeight + 1));

            if (columnIndex < 0)
                columnIndex = 0;

            return GetColumn(columnIndex);
        }

        void SubscribeLayoutManager(CanvasColumnLayoutDrawManager manager, bool subscribe)
        {
            if (manager == null)
                return;

            if (subscribe)
            {
                manager.RowLayoutDrawed += RowLayoutDrawer_RowLayoutDrawed;

                Column column = null;
                if (manager.Columns.Count() > 0)
                    foreach (var rLayout in manager.Columns)
                    {
                        column = GetColumn(rLayout.ColumnIndex);

                        column.ColumnLayout = rLayout;
                    }
            }
            else
            {
                manager.RowLayoutDrawed -= RowLayoutDrawer_RowLayoutDrawed;
            }
        }

        private void M_chart_SetChanged(LeasingSetEventArgs e)
        {
            //SubscribeSet(e.Old, false);

            //SubscribeSet(e.New, true);
        }

        private void RowLayoutDrawer_RowLayoutDrawed(ColumnLayout layout)
        {
            Column column = GetColumn(layout.ColumnIndex);

            column.ColumnLayout = layout;
        }

        /// <summary>
        /// Очистка набора строк
        /// </summary>
        public void Clear()
        {
            m_columns.Clear();
        }

        Column GetColumn(int index)
        {
            Column column = null;
            if (m_columns.ContainsKey(index))
                column = m_columns[index];
            else
            {
                column = new Column(index);
                m_columns.Add(index, column);
            }

            return column;
        }

        public void Dispose()
        {
            foreach (var row in m_columns)
            {
                if (row.Value != null)
                    row.Value.Dispose();
            }

            m_columns.Clear();

            Subscribe(false);

            m_chart = null;
        }

        /// <summary>
        /// вызывается при изменении выбора колонки
        /// </summary>
        /// <param name="column">Строка</param>
        public void CallColumnSelectionChanged(Column column)
        {
            if (column == null)
                return;

            ColumnSelectionChanged?.Invoke(column);
        }
    }
}
