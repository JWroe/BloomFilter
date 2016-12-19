using System.Linq;
using System.Security.Cryptography;
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
            var filter = new BloomFilter(capacity: 1200000u);

            Assert.AreEqual(filter.Capacity, actual: 1200000u);
            Assert.AreEqual(filter.ErrorRate, actual: 0.001);
            Assert.AreEqual(filter.BitsPerSlice, actual: 1725311);
            Assert.AreEqual(filter.NumBits, actual: 17253110);
            Assert.AreEqual(filter.NumSlices, actual: 10);
        }

        [TestMethod]
        public void TestHashingAlgorithm()
        {
            var filter = new BloomFilter(capacity: 1200000u);

            var hashed = filter.HashFunc("0123456789").ToList();

            Assert.AreEqual(expected: 10, actual: hashed.Count);

            Assert.AreEqual(expected: 480376, actual: hashed.ElementAt(index: 0));
            Assert.AreEqual(expected: 1312333, actual: hashed.ElementAt(index: 1));
            Assert.AreEqual(expected: 1352208, actual: hashed.ElementAt(index: 2));
            Assert.AreEqual(expected: 779169, actual: hashed.ElementAt(index: 3));
            Assert.AreEqual(expected: 1055725, actual: hashed.ElementAt(index: 4));
            Assert.AreEqual(expected: 553905, actual: hashed.ElementAt(index: 5));
            Assert.AreEqual(expected: 417018, actual: hashed.ElementAt(index: 6));
            Assert.AreEqual(expected: 26225, actual: hashed.ElementAt(index: 7));
            Assert.AreEqual(expected: 170886, actual: hashed.ElementAt(index: 8));
            Assert.AreEqual(expected: 921791, actual: hashed.ElementAt(index: 9));
        }

        [TestMethod]
        public void UnpackTest()
        {
            //(‘ÁÝ…4Þ+rYŒùX^G»Ã”[zÕ¦øý/ž.þ´Haéô_“çŽS®™2J9iR
            //var input = @"(‘ÁÝ…4Þ+rYŒùX^G»Ã”[zÕ¦øý/ž.þ´Haéô_“çŽS®™2J9iR".GetBytes();
            var input = @"????????????????????????".GetBytes();
            var unpacked = BloomFilter.Unpack(input).ToList();

            Assert.AreEqual(expected: 10, actual: unpacked.Count);

            Assert.AreEqual(expected: 3247515678, actual: unpacked.ElementAt(index: 0));
            Assert.AreEqual(expected: 3727984093, actual: unpacked.ElementAt(index: 1));
            Assert.AreEqual(expected: 1500647467, actual: unpacked.ElementAt(index: 2));
            Assert.AreEqual(expected: 1582889356, actual: unpacked.ElementAt(index: 3));
            Assert.AreEqual(expected: 2495855431, actual: unpacked.ElementAt(index: 4));
            Assert.AreEqual(expected: 2799008347, actual: unpacked.ElementAt(index: 5));
            Assert.AreEqual(expected: 2653945336, actual: unpacked.ElementAt(index: 6));
            Assert.AreEqual(expected: 1219821102, actual: unpacked.ElementAt(index: 7));
            Assert.AreEqual(expected: 1609886049, actual: unpacked.ElementAt(index: 8));
            Assert.AreEqual(expected: 1401874323, actual: unpacked.ElementAt(index: 9));
        }

        [TestMethod]
        public void HashTest()
        {
            var input = "0123456789";
            var expectedOutput = "90ae531f24e48697904a4d0286f354c50a350ebb6c2b9efcb22f71c96ceaeffc11c6095e9ca0df0ec30bf685dcf2e5e5";

            var actual = SHA384.Create().ComputeHash(input.GetBytes()).GetHexString();

            Assert.AreEqual(expectedOutput, actual);
        }
    }
}