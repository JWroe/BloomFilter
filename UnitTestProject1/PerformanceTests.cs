using System;
using System.Diagnostics;
using System.Linq;
using BloomFilters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BloomFilterTests
{
    [TestClass]
    public class PerformanceTests
    {
        private const int Capacity = 1200000;
        private const int AmountToCheck = 1000000;

        [TestMethod]
        public void HashSetTest()
        {
            Trace.WriteLine("");
            Trace.WriteLine("Hash Set Test");
            Trace.WriteLine("-------------");

            var hashSet = new HashSetStore();
            TestDataStructure(hashSet);
        }

        [TestMethod]
        public void BloomFilterTest()
        {
            Trace.WriteLine("");
            Trace.WriteLine("Bloom Filter Test");
            Trace.WriteLine("-----------------");

            var hashSet = new BloomFilter(Capacity);
            TestDataStructure(hashSet);
        }

        private static void TestDataStructure(IHashedDataStore dataStructure)
        {
            var watch = new Stopwatch();

            CheckLoad(dataStructure, watch);

            CheckContains(dataStructure, watch);

            var speed = AmountToCheck / watch.Elapsed.TotalMinutes;
            Trace.WriteLine($"\tCheck time : {watch.Elapsed.TotalSeconds:F} (or {speed / 1000000D:F} million records per minute)");
        }

        private static void CheckLoad(IHashedDataStore dataStructure, Stopwatch watch)
        {
            Trace.WriteLine($"\tloading {Capacity} records into data structure");
            var beforeBytes = GC.GetTotalMemory(forceFullCollection: true);
            watch.Start();
            Enumerable.Range(start: 0, count: Capacity)
                      .Select(i => i.ToString())
                      .ToList()
                      .ForEach(dataStructure.Add);
            watch.Stop();
            var afterBytes = GC.GetTotalMemory(forceFullCollection: true);
            double usage = afterBytes - beforeBytes;
            var suffix = "bytes";
            if (usage > 1024 * 1024)
            {
                usage /= (1024 * 1024D);
                suffix = "MB";
            }
            else if (usage > 1024)
            {
                usage /= (1024D);
                suffix = "KB";
            }
            Trace.WriteLine($"\tEstimated memory usage : {usage:F} {suffix}");
            Trace.WriteLine($"\tLoad time : {watch.Elapsed.TotalSeconds:F} seconds");
        }

        private static void CheckContains(IHashedDataStore dataStructure, Stopwatch watch)
        {
            Trace.WriteLine($"\tChecking {AmountToCheck} records for their presence in the data structure");
            watch.Start();
            for (var i = 0; i < AmountToCheck; i++)
            {
                dataStructure.Contains(i.ToString());
            }
            watch.Stop();
        }
    }
}