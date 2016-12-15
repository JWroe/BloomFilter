using BloomFilters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BloomFilterTests
{
    [TestClass]
    public class BloomFilterTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var filter = new BloomFilter(capacity: 1200000u);

            Assert.AreEqual(filter.Capacity, actual: 1200000u);
            Assert.AreEqual(filter.ErrorRate, actual: 0.001);
            Assert.AreEqual(filter.BitsPerSlice, actual: 1725311);
            Assert.AreEqual(filter.NumBits, actual: 17253110);
            Assert.AreEqual(filter.NumSlices, actual: 10);
        }
    }
}