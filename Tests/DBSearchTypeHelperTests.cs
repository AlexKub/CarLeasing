using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CarLeasingViewer.Tests
{

    [TestClass]
    public class DBSearchTypeHelperTests
    {
        [TestMethod]
        public void GetDefaultValue()
        {
            var val = DBSearchTypeHelper.GetValue("useless_string");

            Assert.IsTrue(DBSearchTypeHelper.DefaultValue == val, "Получено значение, отличное от значения по умолчанию: " + val.ToString());
        }

        [TestMethod]
        public void HasAllDescriptions()
        {
            var enumCount = Enum.GetValues(typeof(DBSearchType)).Length;

            Assert.IsTrue(enumCount == DBSearchTypeHelper.DescriptionsCount, "Не все описания объявлены для перечисления");
        }

        [TestMethod]
        public void CheckAllDescription()
        {
            var descr = DBSearchType.All.GetDescription();

            var awaited = "Все";

            Assert.IsTrue(awaited.Equals(descr), "Полученное описание для значения отличается от ожидаемого. Получено: " + (string.IsNullOrWhiteSpace(descr) ? (descr == null ? "NULL" : "EMPTY") : descr));
        }

        [TestMethod]
        public void CheckOldDescription()
        {
            var descr = DBSearchType.Old.GetDescription();

            var awaited = "Учтённые";

            Assert.IsTrue(awaited.Equals(descr), "Полученное описание для значения отличается от ожидаемого. Получено: " + (string.IsNullOrWhiteSpace(descr) ? (descr == null ? "NULL" : "EMPTY") : descr));
        }

        [TestMethod]
        public void CheckCurentDescription()
        {
            var descr = DBSearchType.Curent.GetDescription();

            var awaited = "Текущие";

            Assert.IsTrue(awaited.Equals(descr), "Полученное описание для значения отличается от ожидаемого. Получено: " + (string.IsNullOrWhiteSpace(descr) ? (descr == null ? "NULL" : "EMPTY") : descr));
        }
    }
}
