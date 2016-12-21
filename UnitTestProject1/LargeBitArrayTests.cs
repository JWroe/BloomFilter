using BloomFilters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BloomFilterTests
{
    [TestClass]
    public class LargeBitArrayTests
    {
        [TestMethod]
        public void Int32Max()
        {
            TestArray(int.MaxValue);
        }

        [TestMethod]
        public void OverInt32Max()
        {
            TestArray(int.MaxValue * 4L);
        }

        private static void TestArray(long size)
        {
            var array = new LargeBitArray(size);

            array.Set(size);
            Assert.IsTrue(array.Get(size));
            Assert.IsFalse(array.Get(index: 0));
        }
    }
}