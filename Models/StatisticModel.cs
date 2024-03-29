﻿using CarLeasingViewer.Controls.LeasingChartManagers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static CarLeasingViewer.Controls.LeasingChartManagers.RowManager;

namespace CarLeasingViewer.Models
{
    /// <summary>
    /// Набор данных статистики для вывода в нижней части окна
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebugDisplay()}")]
    public class StatisticModel : ViewModels.ViewModelBase, IReadOnlyList<StatisticItemModel>
    {
        private IReadOnlyList<StatisticItemModel> pv_Items = new List<StatisticItemModel>();
        /// <summary>
        /// Возвращает или задаёт набор позиций статистики
        /// </summary>
        public IReadOnlyList<StatisticItemModel> Items { get { return pv_Items; } set { if (pv_Items != value) { pv_Items = value; OnPropertyChanged(); HasItems = (value != null) && (value.Count > 0); } } }

        private bool pv_HasItems;
        /// <summary>
        /// Возвращает или задаёт флаг наличия
        /// </summary>
        public bool HasItems { get { return pv_HasItems; } set { if (pv_HasItems != value) { pv_HasItems = value; OnPropertyChanged(); } } }

        /// <summary>
        /// Загрузка статистики по Набору
        /// </summary>
        /// <param name="set">Набор аренды</param>
        public void Load(LeasingSet set)
        {
            if (set == null)
                return;

            var items = new List<StatisticItemModel>();
            items.Add(new StatisticItemModel("Всего машин", set.CarModels.Count.ToString()));

            Items = items;
        }

        /// <summary>
        /// Загрузка статистики по строке
        /// </summary>
        /// <param name="row">Выбранная строка на графике</param>
        public void Load(Row row, LeasingSet set)
        {
            if (row == null)
                return;

            var items = new List<StatisticItemModel>();

            var leasingCount = row.Bars.Sum(b => b.Model == null ? 0 : b.Model.DaysCount);
            var loadPercent = (double)(set.Monthes.Last().Month.LastDate - set.Monthes.First().Month.FirstDate).Days / 100d;
            items.Add(new StatisticItemModel("Авто", row.Car == null ? "NULL" : row.Car.Text));
            items.Add(new StatisticItemModel("Общее время аренды", leasingCount.ToString() + " дн."));
            items.Add(new StatisticItemModel("% загрузки", Math.Round((leasingCount / loadPercent), 2).ToString() + " %"));

            Items = items;
        }

        /// <summary>
        /// Сброс статистики
        /// </summary>
        public void Clear() { Items = new List<StatisticItemModel>(); }

        string DebugDisplay() { return string.Empty; }

        #region IReadOnlyList<StatisticItemModel>

        public int Count => Items.Count;

        public StatisticItemModel this[int index] => Items[index];

        public IEnumerator<StatisticItemModel> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        #endregion


    }
}
