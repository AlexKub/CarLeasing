﻿using CarLeasingViewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace CarLeasingViewer.Controls.LeasingChartManagers
{
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
                if(set.CarModels.Count > 0)
                    foreach (var model in set.CarModels)
                    {
                        if (m_rows.ContainsKey(model.RowIndex))
                            row = m_rows[model.RowIndex];
                        else
                        {
                            row = new Row(model.RowIndex);
                            m_rows.Add(model.RowIndex, row);
                        }

                        row.Car = model;
                    }

                if (set.Comments.Count > 0)
                    foreach (var model in set.Comments)
                    {
                        if (m_rows.ContainsKey(model.RowIndex))
                            row = m_rows[model.RowIndex];
                        else
                        {
                            row = new Row(model.RowIndex);
                            m_rows.Add(model.RowIndex, row);
                        }

                        row.Comment = model;
                    }
            }
            else
            {
                set.CarsChanged -= LeasingSet_CarsChanged;
                set.CommentsChanged -= LeasingSet_CommentsChanged;
            }
        }

        void SubscribeLayoutManager(CanvasRowLayoutDrawManager manager, bool subscribe)
        {
            if (manager == null)
                return;

            if(subscribe)
            {
                manager.RowLayoutDrawed += RowLayoutDrawer_RowLayoutDrawed;

                Row row = null;
                if (manager.Rows.Count() > 0)
                    foreach (var rLayout in manager.Rows)
                    {
                        if (m_rows.ContainsKey(rLayout.RowIndex))
                            row = m_rows[rLayout.RowIndex];
                        else
                        {
                            row = new Row(rLayout.RowIndex);
                            m_rows.Add(rLayout.RowIndex, row);
                        }

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
            Row row = null;

            if (m_rows.ContainsKey(layout.RowIndex))
                row = m_rows[layout.RowIndex];
            else
            {
                row = new Row(layout.RowIndex);
                m_rows.Add(layout.RowIndex, row);
            }

            row.RowLayout = layout;
        }

        private void LeasingSet_CommentsChanged(LeasingSetEventArgs e)
        {
            Row row = null;
            var set = e.New;

            for (int i = 0; i < set.Comments.Count; i++)
            {
                if (m_rows.ContainsKey(i))
                    row = m_rows[i];
                else
                {
                    row = new Row(i);
                    m_rows.Add(i, row);
                }

                row.Comment = set.Comments[i];
            }
        }

        private void LeasingSet_CarsChanged(LeasingSetEventArgs e)
        {
            Row row = null;
            var set = e.New;

            for (int i = 0; i < set.CarModels.Count; i++)
            {
                if (m_rows.ContainsKey(i))
                    row = m_rows[i];
                else
                {
                    row = new Row(i);
                    m_rows.Add(i, row);
                }

                row.Car = set.CarModels[i];
            }
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
        /// Условная строка на графике
        /// </summary>
        public class Row : IDisposable
        {
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

            public Row(int i)
            {
                Index = i;
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

            public void Dispose()
            {
                Car = null;
                RowLayout = null;
                Comment = null;
            }
        }
    }
}
