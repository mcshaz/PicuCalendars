using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PicuCalendars.Utilities;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace TestCalendar
{
    [TestClass]
    public class TestLinqGrouptensions
    {
        [TestMethod]
        public void TestIncrementalIntegerGroups()
        {
            var test = new[] { 1, 2, 3, 6, 7, 10 };
            DeepCompare(new[] { new[] { 1, 2, 3 }, new[] { 6, 7 }, new[] { 10 } }, test.ConsecutiveGroup((prior,cur)=> cur-prior == 1).ToList().Cast<ICollection>().ToList());
        }

        private static void DeepCompare(IList<ICollection> expected, IList<ICollection> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);
            for (int i =0;i< expected.Count; i++)
            {
                CollectionAssert.AreEqual(expected[i], actual[i]);
            }
        }

        private readonly Func<int, int, bool> _isIncremental = (prior, curr) => curr - prior == 1;

        [TestMethod]
        public void TestIncrementalIntegersAll()
        {
            IList<int> test = new[] { 1, 2, 3, 6, 7, 10 };
            Assert.IsFalse(test.ConsecutiveAll(_isIncremental));
            test = Enumerable.Range(2, 6).ToList();
            Assert.IsTrue(test.ConsecutiveAll(_isIncremental));
        }
    }
}
