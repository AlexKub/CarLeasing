using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using CarLeasingViewer;

namespace CarLeasingViewer.Tests
{
    [TestClass]
    public class ExtensionsTests
    {
        [TestMethod]
        public void IsEmpty_NullRef()
        {
            IEnumerable<int> collection = null;

            var res = collection.IsEmpty();

            Assert.IsTrue(res, "Коллекция не пуста");
        }

        [TestMethod]
        public void IsEmpty_Empty()
        {
            IEnumerable<int> collection = new List<int>();

            var res = collection.IsEmpty();

            Assert.IsTrue(res, "Коллекция не пуста");
        }

        [TestMethod]
        public void IsEmpty_NotEmpty()
        {
            IEnumerable<int> collection = new List<int>() { 1 };

            var res = collection.IsEmpty();

            Assert.IsFalse(res, "Коллекция пуста");
        }
    }
}
