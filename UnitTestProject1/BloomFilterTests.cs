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

            Assert.AreEqual(filter.Capacity, actual: 1200000u);
            Assert.AreEqual(filter.ErrorRate, actual: 0.001);
            Assert.AreEqual(filter.BitsPerSlice, actual: 1725311u);
            Assert.AreEqual(filter.NumBits, actual: 17253110u);
            Assert.AreEqual(filter.NumSlices, actual: 10u);
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
    }
}