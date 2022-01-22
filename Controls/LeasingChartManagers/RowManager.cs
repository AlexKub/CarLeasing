using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CarLeasingViewer.Controls.LeasingChartManagers
{
    public delegate void RowSelectedEvent(Row row);

    /// <summary>
    /// Поддержка абстракции строк на графике. Зависит от инициализации прочих менеджеров
    /// </summary>
    public class RowManager : IDisposable
    {
        LeasingChart m_chart;

        readonly Dictionary<int, Row> m_rows = new Dictionary<int, Row>();

        /// <summary>
        /// Получение строки по индексу
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Row this[int index] { get { return m_rows.ContainsKey(index) ? m_rows[index] : null; } }

        /// <summary>
        /// Строки
        /// </summary>
        public IEnumerable<Row> Rows { get { return m_rows.Values; } }

        /// <summary>
        /// Количество строк
        /// </summary>
        public int Count { get { return m_rows.Count; } }

        public event RowSelectedEvent RowSelectionChanged;

        public RowManager(LeasingChart chart)
        {
            m_chart = chart;

            Subscribe(true);
        }

        void Subscribe(bool subscribe)
        {
            if (m_chart == null)
                return;

            SubscribeSet(m_chart.LeasingSet, subscribe);

            SubscribeLayoutManager(m_chart.RowLayoutDrawer, subscribe);

            SubscribeBarManager(m_chart.BarManager, subscribe);

            if (subscribe)
            {
                m_chart.SetChanged += M_chart_SetChanged;
            }
            else
            {
                m_chart.SetChanged -= M_chart_SetChanged;
            }
        }

        void SubscribeSet(LeasingSet set, bool subscribe)
        {
            if (set == null)
                return;

            if (subscribe)
            {
                set.CarsChanged += LeasingSet_CarsChanged;
                set.CommentsChanged += LeasingSet_CommentsChanged;

                Row row = null;
                if (set.CarModels.Count > 0)
                    foreach (var model in set.CarModels)
                    {
                        row = GetRow(model.RowIndex);

                        row.Car = model;
                    }

                if (set.Comments.Count > 0)
                    foreach (var model in set.Comments)
                    {
                        row = GetRow(model.RowIndex);

                        row.Comment = model;
                    }
            }
            else
            {
                set.CarsChanged -= LeasingSet_CarsChanged;
                set.CommentsChanged -= LeasingSet_CommentsChanged;
            }
        }

        void SubscribeBarManager(CanvasBarDrawManager manager, bool subscribe)
        {
            if (manager == null)
                return;

            if (subscribe)
            {
                manager.BarAdded += Manager_BarAdded; ;

                Row row = null;
                if (manager.Bars.Count() > 0)
                    foreach (var bar in manager.Bars)
                    {
                        row = GetRow(bar.Index);

                        row.Add(bar);
                    }
            }
            else
            {
                manager.BarAdded -= Manager_BarAdded;
            }
        }

        /// <summary>
        /// Обработка отрисовки нового прямоугольника на графике
        /// </summary>
        /// <param name="bar"></param>
        private void Manager_BarAdded(CanvasBarDrawManager.BarData bar)
        {
            Row row = GetRow(bar.Index);

            //проставляем z-index для наслаивающихся друг на друга полосок
            if(row.Bars.Count > 0)
            {
                foreach (var b in row.Bars)
                {
                    if (b.Bar.IntersectsWith(bar.Bar))
                        bar.ZIndex++;
                }
            }

            //добавляем прямоугольник к текущей строке
            row.Add(bar);
        }

        /// <summary>
        /// Получение строки по координатам
        /// </summary>
        /// <param name="p">Искомая точка</param>
        /// <returns>Возвращает строку, к которой принадлежт точка или null</returns>
        public Row GetRowByPoint(Point p)
        {
            var rowIndex = (int)(p.Y / (m_chart.RowHeight + 1));

            if (rowIndex < 0)
                rowIndex = 0;

            return GetRow(rowIndex);
        }

        void SubscribeLayoutManager(CanvasRowLayoutDrawManager manager, bool subscribe)
        {
            if (manager == null)
                return;

            if (subscribe)
            {
                manager.RowLayoutDrawed += RowLayoutDrawer_RowLayoutDrawed;

                Row row = null;
                if (manager.Rows.Count() > 0)
                    foreach (var rLayout in manager.Rows)
                    {
                        row = GetRow(rLayout.RowIndex);

                        row.RowLayout = rLayout;
                    }
            }
            else
            {
                manager.RowLayoutDrawed -= RowLayoutDrawer_RowLayoutDrawed;
            }
        }

        private void M_chart_SetChanged(LeasingSetEventArgs e)
        {
            SubscribeSet(e.Old, false);

            SubscribeSet(e.New, true);
        }

        private void RowLayoutDrawer_RowLayoutDrawed(RowLayout layout)
        {
            Row row = row = GetRow(layout.RowIndex);

            row.RowLayout = layout;
        }

        private void LeasingSet_CommentsChanged(LeasingSetEventArgs e)
        {
            Row row = null;
            var set = e.New;

            for (int i = 0; i < set.Comments.Count; i++)
            {
                row = GetRow(i);

                row.Comment = set.Comments[i];
            }
        }

        private void LeasingSet_CarsChanged(LeasingSetEventArgs e)
        {
            Row row = null;
            var set = e.New;

            for (int i = 0; i < set.CarModels.Count; i++)
            {
                row = GetRow(i);

                row.Car = set.CarModels[i];
            }
        }

        /// <summary>
        /// Очистка набора строк
        /// </summary>
        public void Clear()
        {
            m_rows.Clear();
        }

        Row GetRow(int index)
        {
            Row row = null;
            if (m_rows.ContainsKey(index))
                row = m_rows[index];
            else
            {
                row = new Row(index);
                m_rows.Add(index, row);
            }

            return row;
        }

        public void Dispose()
        {
            foreach (var row in m_rows)
            {
                if (row.Value != null)
                    row.Value.Dispose();
            }

            m_rows.Clear();

            Subscribe(false);

            m_chart = null;
        }

        /// <summary>
        /// вызывается при изменении выбора строки
        /// </summary>
        /// <param name="row">Строка</param>
        public void CallRowSelectionChanged(Row row)
        {
            if (row == null)
                return;

            RowSelectionChanged?.Invoke(row);
        }
    }
}
