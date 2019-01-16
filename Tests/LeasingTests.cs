using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CarLeasingViewer.Tests
{
    [TestClass]
    public class LeasingTests
    {
        [TestMethod]
        public void Cross_Full()
        {
            var l = new Models.Leasing();
            l.DateStart = DateTime.Now;

            l.DateEnd = DateTime.Now.AddDays(1);

            Assert.IsTrue(l.Cross(DateTime.Now, DateTime.Now.AddDays(1)), "Не найдено пересечения");
        }

        [TestMethod]
        public void Cross_Start()
        {
            var l = new Models.Leasing();
            l.DateStart = DateTime.Now;

            l.DateEnd = DateTime.Now.AddDays(3);

            Assert.IsTrue(l.Cross(DateTime.Now.AddDays(-1), DateTime.Now), "Не найдено пересечения");
        }


        [TestMethod]
        public void Cross_End()
        {
            var l = new Models.Leasing();
            l.DateStart = DateTime.Now;

            l.DateEnd = DateTime.Now.AddDays(4);

            Assert.IsTrue(l.Cross(DateTime.Now.AddDays(-4), DateTime.Now.AddDays(4)), "Не найдено пересечения");
        }

        [TestMethod]
        public void Cross_False()
        {
            var l = new Models.Leasing();
            l.DateStart = DateTime.Now;

            l.DateEnd = DateTime.Now.AddDays(4);

            Assert.IsFalse(l.Cross(DateTime.Now.AddDays(-4), DateTime.Now.AddDays(-1)), "Найдено лишнее пересечение");

            Assert.IsFalse(l.Cross(DateTime.Now.AddDays(5), DateTime.Now.AddDays(6)), "Найдено лишнее пересечение");
        }
    }
}
