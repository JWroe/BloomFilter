using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;

namespace BloomFilters
{
    public class BloomFilter
    {
        public uint NumBits => NumSlices * BitsPerSlice;

        public uint Capacity { get; set; }

        public uint BitsPerSlice { get; set; }

        public uint NumSlices { get; set; }

        public double ErrorRate { get; set; }

        public Func<string, IEnumerable<uint>> HashFunc { get; private set; }

        public Func<string, IEnumerable<uint>> CreateHashingAlgorithm()
        {
            const int chunkSize = 4;

            var totalHashBits = 8 * NumSlices * chunkSize;
            var algorithm = ChooseHashAlgorithm(totalHashBits);

            var count = (int)Math.Floor((double)algorithm.HashSize / chunkSize);
            var numSalts = NumSlices / count;
            var extra = NumSlices % count;
            if (extra != 0)
            {
                numSalts++;
            }

            var hashFuncCreator = HashFunction.CreateHashFunction(algorithm);
            var salts = Enumerable.Range(start: 0, count: (int)numSalts).Select(i => hashFuncCreator(hashFuncCreator(i.ToString().GetBytes()).Digest()));

            return key => Hash(NumSlices, salts, key);
        }

        private IEnumerable<uint> Hash(uint numSlices, IEnumerable<HashFunction> salts, string key)
        {
            var i = 0;
            foreach (var salt in salts)
            {
                var h = salt.Copy();
                h.Update(key);
                foreach (var num in Unpack(h.Digest()))
                {
                    yield return num % BitsPerSlice;
                    i++;
                    if (i >= numSlices)
                    {
                        break;
                    }
                }
            }
        }

        public IEnumerable<uint> Unpack(byte[] bytes)
        {
            const int size = sizeof(uint);

            for (var i = 0; i < NumSlices; i++)
            {
                var value = bytes.Skip(i * size).Take(size).ToArray();
                yield return BitConverter.ToUInt32(value, startIndex: 0);
            }
        }

        private static HashAlgorithm ChooseHashAlgorithm(long totalHashBits)
        {
            HashAlgorithm algorithm;
            if (totalHashBits > 384)
            {
                algorithm = SHA512.Create();
            }
            else if (totalHashBits > 256)
            {
                algorithm = SHA384.Create();
            }
            else if (totalHashBits > 160)
            {
                algorithm = SHA256.Create();
            }
            else if (totalHashBits > 128)
            {
                algorithm = SHA1.Create();
            }
            else
            {
                algorithm = MD5.Create();
            }
            return algorithm;
        }

        public BloomFilter(uint capacity, double errorRate = 0.001)
        {
            var numSlices = (uint)Math.Ceiling(Math.Log(1.0 / errorRate, newBase: 2));

            BitsPerSlice = CalculateBitsPerSlice(capacity, errorRate, numSlices);
            ErrorRate = errorRate;
            NumSlices = numSlices;
            Capacity = capacity;
            HashFunc = CreateHashingAlgorithm();
        }

        private static uint CalculateBitsPerSlice(uint capacity, double errorRate, uint numSlices)
        {
            var bitsNumerator = capacity * Math.Abs(Math.Log(errorRate));
            var bitsDenominator = numSlices * Math.Pow(Math.Log(2), y: 2);
            return (uint)Math.Ceiling(bitsNumerator / bitsDenominator);
        }
    }
}