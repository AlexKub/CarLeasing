using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace CarLeasingViewer.Controls.LeasingChartManagers
{
    public delegate void RowSelectedEvent(RowManager.Row row);

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

        private void Manager_BarAdded(CanvasBarDrawManager.BarData bar)
        {
            Row row = GetRow(bar.Index);

            row.Add(bar);
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

        private void RowLayoutDrawer_RowLayoutDrawed(CanvasRowLayoutDrawManager.RowLayout layout)
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

        /// <summary>
        /// Условная строка на графике
        /// </summary>
        [System.Diagnostics.DebuggerDisplay("{DebugerDisplay()}")]
        public class Row : IDisposable
        {
            List<CanvasBarDrawManager.BarData> m_Bars = new List<CanvasBarDrawManager.BarData>();

            /// <summary>
            /// Индекс строки (начиная с 0)
            /// </summary>
            public int Index { get; private set; }

            /// <summary>
            /// Данные о машине
            /// </summary>
            public CarModel Car { get; set; }

            /// <summary>
            /// Layout строки
            /// </summary>
            public CanvasRowLayoutDrawManager.RowLayout RowLayout { get; set; }

            /// <summary>
            /// Данные полосок, принадлежащих текущей строке графика
            /// </summary>
            public IReadOnlyList<CanvasBarDrawManager.BarData> Bars { get { return m_Bars; } }

            /// <summary>
            /// Комментарий
            /// </summary>
            public CarCommentModel Comment { get; set; }

            HightlightManager.HightlightAction m_hState;
            /// <summary>
            /// Текущее состояние подсветки строки
            /// </summary>
            public HightlightManager.HightlightAction HightlightState
            {
                get { return m_hState; }
                set
                {
                    m_hState = value;
                    SetHighlightFlag(value != HightlightManager.HightlightAction.None);
                }
            }

            /// <summary>
            /// Флаг, что строка была выбрана пользователем
            /// </summary>
            public bool Selected { get; set; }

            public Row(int i) { Index = i; }

            public void Add(CanvasBarDrawManager.BarData bar)
            {
                m_Bars.Add(bar);
            }

            /// <summary>
            /// Подсветка строки
            /// </summary>
            /// <param name="brush">Кисть подсветки</param>
            public void Hightlight(Brush brush)
            {
                if (Car != null)
                    Car.HightlightBrush = brush;
                if (RowLayout != null)
                    RowLayout.HightlightBrush = brush;
                if (Comment != null)
                    Comment.HightlightBrush = brush;
            }

            /// <summary>
            /// Простановка флага "подсвечено"
            /// </summary>
            /// <param name="hightlight">Флаг</param>
            void SetHighlightFlag(bool hightlight)
            {
                if (Car != null)
                    Car.Hightlighted = hightlight;
                if (RowLayout != null)
                    RowLayout.Hightlighted = hightlight;
                if (Comment != null)
                    Comment.Hightlighted = hightlight;
            }

            public void Clear()
            {
                Car = null;
                RowLayout = null;
                Comment = null;
                m_Bars.Clear();
            }

            public void Dispose()
            {
                Clear();
            }

            string DebugerDisplay()
            {
                return Index.ToString() + " | " + (Car == null ? "NULL" : string.IsNullOrEmpty(Car.Text) ? "EMPTY" : Car.Text);
            }
        }
    }
}
