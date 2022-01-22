using System;
using System.Collections.Generic;
using System.Linq;

namespace CarLeasingViewer
{
    public class SortService
    {
        readonly List<Controls.LeasingChartManagers.Row> m_sortedRows = new List<Controls.LeasingChartManagers.Row>();

        readonly List<Controls.LeasingChartManagers.Row> m_totalRows = new List<Controls.LeasingChartManagers.Row>();

        /// <summary>
        /// Сортировка моделей по дате
        /// </summary>
        /// <param name="date">Выбранная дата</param>
        public void Sort(DateTime date)
        {
            var rows = m_totalRows;

            if (rows != null && rows.Count() > 0)
            {
                foreach (var row in rows)
                {
                    if (row.Bars.Count > 0)
                    {
                        foreach (var bar in row.Bars)
                        {
                            if (bar.Model == null || bar.Model.Leasing == null)
                                continue;

                            if (bar.Model.Leasing.Include(date))
                            {
                                m_sortedRows.Add(row);
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Сортировка моделей по дате
        /// </summary>
        /// <param name="date">Выбранная дата</param>
        public void Sort(DateTime dateStart, DateTime dateEnd)
        {
            var rows = m_totalRows;

            if (rows != null && rows.Count() > 0)
            {
                var sorted = new List<Controls.LeasingChartManagers.Row>();
                foreach (var row in rows)
                {
                    if (row.Bars.Count > 0)
                    {
                        foreach (var bar in row.Bars)
                        {
                            if (bar.Model == null || bar.Model.Leasing == null)
                                continue;

                            if (bar.Model.Leasing.DateStart > dateEnd)
                                break;

                            if (bar.Model.Leasing.Cross(dateStart, dateEnd))
                            {
                                sorted.Add(row);
                                break;
                            }
                        }
                    }
                }
            }
        }

        #region sorting

        #endregion
    }
}
