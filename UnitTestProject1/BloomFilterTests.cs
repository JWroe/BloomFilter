using System;
using System.Diagnostics;
using System.Linq;
using BloomFilters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BloomFilterTests
{
    [TestClass]
    public class BloomFilterTests
    {
        [TestMethod]
        public void ConfigTest()
        {
            var filter = new BloomFilter(capacity: 1200000);

            Assert.AreEqual(filter.Capacity, actual: 1200000);
            Assert.AreEqual(filter.ErrorRate, actual: 0.001);
            Assert.AreEqual(filter.BitsPerSlice, actual: 1725311);
            Assert.AreEqual(filter.NumBits, actual: 17253110);
            Assert.AreEqual(filter.NumSlices, actual: 10);
        }

        [TestMethod]
        public void TestHashingAlgorithm()
        {
            var filter = new BloomFilter(capacity: 1200000);

            var hashed = filter.HashFunc("0123456789").ToList();

            Assert.AreEqual(expected: 10, actual: hashed.Count);

            Assert.AreEqual(expected: 480376u, actual: hashed.ElementAt(index: 0));
            Assert.AreEqual(expected: 1312333u, actual: hashed.ElementAt(index: 1));
            Assert.AreEqual(expected: 1352208u, actual: hashed.ElementAt(index: 2));
            Assert.AreEqual(expected: 779169u, actual: hashed.ElementAt(index: 3));
            Assert.AreEqual(expected: 1055725u, actual: hashed.ElementAt(index: 4));
            Assert.AreEqual(expected: 553905u, actual: hashed.ElementAt(index: 5));
            Assert.AreEqual(expected: 417018u, actual: hashed.ElementAt(index: 6));
            Assert.AreEqual(expected: 26225u, actual: hashed.ElementAt(index: 7));
            Assert.AreEqual(expected: 170886u, actual: hashed.ElementAt(index: 8));
            Assert.AreEqual(expected: 921791u, actual: hashed.ElementAt(index: 9));
        }

        [TestMethod]
        public void KeysAddedCanBeFound()
        {
            const string input = "0123456789";
            var filter = new BloomFilter(capacity: 1200000);
            filter.Add(input);
            Assert.IsTrue(filter.Contains(input));
            Assert.IsFalse(filter.Contains("012345678"));
        }

        [TestMethod]
        public void PerformanceTest()
        {
            const int capacity = 1200000;
            var filter = new BloomFilter(capacity);
            Enumerable.Range(start: 0, count: capacity)
                      .Select(i => i.ToString())
                      .ToList()
                      .ForEach(i => filter.Add(i));

            const int amountToCheck = 100000;
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < amountToCheck; i++)
            {
                filter.Contains(i.ToString());
            }
            watch.Stop();
            var allowedTime = TimeSpan.FromMinutes(amountToCheck / 1000000D);
            Assert.IsTrue(watch.Elapsed <= allowedTime, watch.Elapsed.ToString());
        }
    }
}