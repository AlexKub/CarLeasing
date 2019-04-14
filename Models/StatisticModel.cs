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

            //аренды
            var leasings = row.Bars.Where(b => b.Model != null).Select(b => b.Model).OfType<LeasingBarModel>();
            var leasingCount = leasings.Sum(l => set.CrossDaysCount(l.Leasing));

            //сторнирование
            var stornos = row.Bars.Where(b => b.Model != null).Select(b => b.Model).OfType<StornoBarModel>();
            var stornosCount = stornos.Sum(s => set.CrossDaysCount(s.Period));

            //ремонт
            var maintenances = row.Bars.Where(b => b.Model != null).Select(b => b.Model).OfType<MaintenanceBarModel>();
            var maintenancesCount = maintenances.Sum(m => set.CrossDaysCount(m.Period));

            var loadPercent = (set.Monthes.Last().Month.LastDate - set.Monthes.First().Month.FirstDate).Days / 100d;
            items.Add(new StatisticItemModel("Авто", row.Car == null ? "NULL" : row.Car.Text));

            items.Add(new StatisticItemModel("Общее время аренды", (leasingCount).ToString() + " дн."));
            if (stornosCount > 0)
                items.Add(new StatisticItemModel("Сторнированное время", (stornosCount).ToString() + " дн."));
            if (maintenancesCount > 0)
                items.Add(new StatisticItemModel("Время ремонта", (maintenancesCount).ToString() + " дн."));
            items.Add(new StatisticItemModel("% загрузки", Math.Round((leasingCount / loadPercent), 2).ToString() + " %"));

            var model = row.Car;

            if (model != null)
            {
                //получаем свежую цену из БД
                model.UpdatePrice();

                //если указана цена
                if (!(model?.Price.IsNullOrEmpty() ?? false))
                {
                    var price = row.Car.Price;

                    const string format = "F2";

                    Func<decimal, string> convert = (p) => { return p.ToString(format) + " р."; };

                    if (price.Day != decimal.Zero)
                        items.Add(new StatisticItemModel("От 1 до 2 дней", convert(price.Day)));

                    if (price.Week != decimal.Zero)
                        items.Add(new StatisticItemModel("От 3 до 6 дней", convert(price.Week)));

                    if (price.Long != decimal.Zero)
                        items.Add(new StatisticItemModel("Более 7 дней", convert(price.Long)));
                }
            }

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
