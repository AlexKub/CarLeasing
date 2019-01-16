using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CarLeasingViewer;
using carRow = CarLeasingViewer.Controls.LeasingChartManagers.RowManager.Row;
using CarLeasingViewer.Models;

namespace CarLeasingViewer.Tests
{
    [TestClass]
    public class SortManagerTests
    {
        [TestMethod]
        public void SelectFree_Null()
        {
            IEnumerable<carRow> nullRef = null;

            var dt = DateTime.Now;
            var res = nullRef.SelectFree(dt, dt);

            Assert.IsNotNull(res, "Возвращена пустая ссылка в ответ");

            Assert.IsTrue(res.Count == 0, "Итоговая коллекция не пуста");
        }

        [TestMethod]
        public void SelectFree_Empty()
        {
            List<carRow> empty = new List<carRow>();

            var dt = DateTime.Now;
            var res = empty.SelectFree(dt, dt);

            Assert.IsNotNull(res, "Возвращена пустая ссылка в ответ");

            Assert.IsTrue(res.Count == 0, "Итоговая коллекция не пуста");
        }

        [TestMethod]
        public void SelectFree_DifferentReference()
        {
            List<carRow> empty = new List<carRow>();

            var dt = DateTime.Now;
            var res = empty.SelectFree(dt, dt);

            empty.Add(new carRow(0));

            Assert.IsTrue(res.Count == 0, "Возвращена ссылка на ту же коллекцию");

            var mock = GetMockCollection(10, 11);
            var baseCount = mock.Count;

            res = mock.SelectFree(DateTime.Now, DateTime.Now);

            res.Add(GenerateMockRow(mock.Select(m => m.Index).Max() + 1));

            Assert.IsTrue(baseCount == mock.Count, "Возвращена ссылка на ту же коллекцию");
        }


        [TestMethod]
        public void SelectFree_Sort_Exists()
        {
            List<carRow> collection = new List<carRow>();

            var now = DateTime.Now;

            var rowCounter = 0;
            collection.Add(GenerateMockRow(rowCounter++, GenerateMockBar(now.AddDays(-10), now.AddDays(-1))));
            collection.Add(GenerateMockRow(rowCounter++, GenerateMockBar(now, now.AddDays(10))));
            collection.Add(GenerateMockRow(rowCounter++, GenerateMockBar(now.AddDays(1), now.AddDays(10))));

            var res = collection.SelectFree(now, now);

            Assert.IsTrue(res.Count == 2, $"Результат сортировки ({res.Count.ToString()}) не совпадает с ожидаемым");
        }

        [TestMethod]
        public void SelectFree_Sort_NotExists()
        {
            List<carRow> collection = new List<carRow>();

            var now = DateTime.Now;

            var rowCounter = 0;
            collection.Add(GenerateMockRow(rowCounter++, GenerateMockBar(now.AddDays(-10), now.AddDays(-1))));
            collection.Add(GenerateMockRow(rowCounter++, GenerateMockBar(now.AddDays(1), now.AddDays(10))));

            var res = collection.SelectFree(now, now);

            Assert.IsTrue(res.Count == 2, $"Результат сортировки ({res.Count.ToString()}) не совпадает с ожидаемым");
        }

        List<carRow> GetMockCollection(int minCount = 100, int maxCount = 1000)
        {
            var rand = new Random(12345);

            var list = new List<carRow>();

            var listCount = rand.Next(minCount, maxCount);
            DateTime dateEnd = DateTime.Now;
            for (int i = 0; i < listCount; i++)
            {
                var row = GenerateMockRow(i);

                list.Add(row);
            }

            return list;
        }

        carRow GenerateMockRow(int index)
        {
            var rand = new Random(index);

            var row = new carRow(index);

            var leasingsCount = rand.Next(0, 11);

            DateTime dateEnd = DateTime.Now.AddDays(-100);
            for (int j = 0; j < leasingsCount; j++)
            {
                var dateStart = dateEnd.AddDays(rand.Next(0, 32));
                dateEnd = dateStart.AddDays(rand.Next(0, 15));

                var leasingModel = GenerateMockBar(dateStart, dateEnd);
                row.Add(leasingModel);
            }

            return row;
        }

        carRow GenerateMockRow(int index, Controls.LeasingChartManagers.CanvasBarDrawManager.BarData bar)
        {
            var row = new carRow(index);

            row.Add(bar);

            return row;
        }

        Controls.LeasingChartManagers.CanvasBarDrawManager.BarData GenerateMockBar(DateTime dateStart, DateTime dateEnd)
        {
            var leasingModel = new Models.LeasingElementModel();
            leasingModel.Monthes = Month.GetMonthes(dateStart, dateEnd).Select(m => new MonthHeaderModel() { Month = m }).ToArray();
            leasingModel.Leasing = new Models.Leasing() { DateStart = dateStart, DateEnd = dateEnd };
            var bar = new Controls.LeasingChartManagers.CanvasBarDrawManager.BarData(leasingModel);

            return bar;
        }
    }
}
