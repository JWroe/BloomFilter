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

        [TestMethod]
        public void UnpackTest()
        {
            var input = "1e2891c1dd8534de2b1072598cf9585e47bbc3945b7ad5a6f8fd2f9e2efeb44861e9f45f93e78e53ae9907324a396952".GetBytesFromHexString();

            var filter = new BloomFilter(capacity: 1200000);
            var unpacked = filter.Unpack(input).ToList();

            Assert.AreEqual(expected: 10, actual: unpacked.Count);

            Assert.AreEqual(expected: 3247515678u, actual: unpacked.ElementAt(index: 0));
            Assert.AreEqual(expected: 3727984093u, actual: unpacked.ElementAt(index: 1));
            Assert.AreEqual(expected: 1500647467u, actual: unpacked.ElementAt(index: 2));
            Assert.AreEqual(expected: 1582889356u, actual: unpacked.ElementAt(index: 3));
            Assert.AreEqual(expected: 2495855431u, actual: unpacked.ElementAt(index: 4));
            Assert.AreEqual(expected: 2799008347u, actual: unpacked.ElementAt(index: 5));
            Assert.AreEqual(expected: 2653945336u, actual: unpacked.ElementAt(index: 6));
            Assert.AreEqual(expected: 1219821102u, actual: unpacked.ElementAt(index: 7));
            Assert.AreEqual(expected: 1609886049u, actual: unpacked.ElementAt(index: 8));
            Assert.AreEqual(expected: 1401874323u, actual: unpacked.ElementAt(index: 9));
        }

        [TestMethod]
        public void GetBytesTest()
        {
            const string toHash = "00123456789";
            var expectedBytes = new byte[]
                                {
                                    2, 107, 151, 248, 95, 123, 19, 211, 29, 183, 226, 177, 207, 219, 53, 246, 205, 154, 27, 186, 58, 238, 102, 232, 188, 249, 10, 58, 197, 104, 58, 219, 83, 17,
                                    193, 6, 50, 143, 254, 144, 252, 210, 161, 205, 172, 32, 97, 109
                                };

            var hashed = SHA384.Create().ComputeHash(toHash.GetBytes());

            for (var i = 0; i < expectedBytes.Length; i++)
            {
                Assert.AreEqual(hashed[i], expectedBytes[i]);
            }

            Assert.AreEqual(hashed.Length, expectedBytes.Length);
        }

        [TestMethod]
        public void GetHexStringTest()
        {
            const string toHash = "00123456789";
            const string expectedHexResult = @"026b97f85f7b13d31db7e2b1cfdb35f6cd9a1bba3aee66e8bcf90a3ac5683adb5311c106328ffe90fcd2a1cdac20616d";

            var hashed = SHA384.Create().ComputeHash(toHash.GetBytes()).GetHexString();

            Assert.AreEqual(expectedHexResult.ToUpper(), hashed.ToUpper());
        }
    }
}