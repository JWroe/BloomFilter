using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace BloomFilters
{
    public class BloomFilter
    {
        private LargeBitArray[] BitArray { get; }

        public int NumBits => NumSlices * BitsPerSlice;

        public int Capacity { get; set; }

        public int BitsPerSlice { get; set; }

        public int NumSlices { get; set; }

        public double ErrorRate { get; set; }

        public Func<string, IEnumerable<uint>> HashFunc { get; }

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
            var salts = Enumerable.Range(start: 0, count: numSalts).Select(i => hashFuncCreator(hashFuncCreator(i.ToString().GetBytes()).Digest()));

            return key => Hash(salts, key);
        }

        private IEnumerable<uint> Hash(IEnumerable<HashFunction> salts, string key)
        {
            return salts.Select(salt =>
                                {
                                    var h = salt.Copy();
                                    h.Update(key);
                                    return h.Digest()
                                            .UnpackToUInt()
                                            .Select(num => num % (uint)BitsPerSlice)
                                            .ToList();
                                })
                        .Aggregate((uints, uints2) => uints.Combine(uints2))
                        .Take(NumSlices);
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

        public BloomFilter(int capacity, double errorRate = 0.001)
        {
            var numSlices = (int)Math.Ceiling(Math.Log(1.0 / errorRate, newBase: 2));

            BitsPerSlice = CalculateBitsPerSlice(capacity, errorRate, numSlices);
            ErrorRate = errorRate;
            NumSlices = numSlices;
            Capacity = capacity;
            HashFunc = CreateHashingAlgorithm();
            BitArray = Enumerable.Range(start: 0, count: NumSlices).Select(i => new LargeBitArray(BitsPerSlice)).ToArray();
        }

        private static int CalculateBitsPerSlice(int capacity, double errorRate, int numSlices)
        {
            var bitsNumerator = capacity * Math.Abs(Math.Log(errorRate));
            var bitsDenominator = numSlices * Math.Pow(Math.Log(2), y: 2);
            return (int)Math.Ceiling(bitsNumerator / bitsDenominator);
        }

        public void Add(string input)
        {
            var hashed = HashFunc(input).ToList();

            for (var i = 0; i < NumSlices; i++)
            {
                BitArray[i].Set(hashed.ElementAt(i));
            }
        }

        public bool Contains(string input)
        {
            var hashed = HashFunc(input).ToList();
            var result = false;
            for (var i = 0; i < NumSlices; i++)
            {
                result |= BitArray[i].Get(hashed.ElementAt(i));
            }

            return result;
        }
    }
}