using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace BloomFilters
{
    public class BloomFilter
    {
        //public Func<string, object> MakeHashes => MakeHashFuncs(NumSlices, BitsPerSlice);
        public int NumBits => NumSlices * BitsPerSlice;

        public ulong Capacity { get; set; }

        public int BitsPerSlice { get; set; }

        public int NumSlices { get; set; }

        public double ErrorRate { get; set; }

        public Func<string, IEnumerable<long>> CreateHashingAlgorithm(int numSlices, long numBits)
        {
            int chunkSize;
            char fmtCode;
            if (numBits > int.MaxValue)
            {
                chunkSize = 8;
                fmtCode = 'Q';
            }
            else if (numBits > short.MaxValue)
            {
                chunkSize = 4;
                fmtCode = 'I';
            }
            else
            {
                chunkSize = 2;
                fmtCode = 'H';
            }

            var totalHashBits = 8 * numSlices * chunkSize;
            var algorithm = ChooseHashAlgorithm(totalHashBits);

            var count = (int)Math.Floor((double)algorithm.HashSize / chunkSize);
            var fmt = Enumerable.Repeat(fmtCode, count);
            var numSalts = numSlices / count;
            var extra = numSlices % count;
            if (extra != 0)
            {
                numSalts++;
            }

            //salts = tuple(hashfn(hashfn(str(i)).digest()) for i in xrange(num_salts))
            var hashFuncCreator = HashFunction.CreateHashFunction(algorithm);
            var salts = Enumerable.Range(start: 0, count: numSalts).Select(i => hashFuncCreator(hashFuncCreator(i.ToString().GetBytes()).Digest()));

            //    def _make_hashfuncs(key):
            //if isinstance(key, unicode):
            //    key = key.encode('utf-8')
            //else:
            //    key = str(key)
            //i = 0
            //for salt in salts:
            //    h = salt.copy()
            //    h.update(key)
            //    for uint in unpack(fmt, h.digest()):
            //        yield uint % num_bits
            //        i += 1
            //        if i >= num_slices:
            //            return

            return key => Hash(numSlices, numBits, salts, key);
        }

        private static IEnumerable<long> Hash(int numSlices, long numBits, IEnumerable<HashFunction> salts, string key)
        {
            var i = 0;
            foreach (var salt in salts)
            {
                var h = salt.Copy();
                h.Update(key);

                foreach (long num in h.Digest())
                {
                    yield return num % numBits;
                    i++;
                    if (i >= numSlices)
                    {
                        break;
                    }
                }
            }
        }

        private static HashAlgorithm ChooseHashAlgorithm(int totalHashBits)
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

        public BloomFilter(ulong capacity, double errorRate = 0.001)
        {
            var numSlices = (int)Math.Ceiling(Math.Log(1.0 / errorRate, newBase: 2));

            BitsPerSlice = CalculateBitsPerSlice(capacity, errorRate, numSlices);
            ErrorRate = errorRate;
            NumSlices = numSlices;
            Capacity = capacity;
        }

        private static int CalculateBitsPerSlice(ulong capacity, double errorRate, int numSlices)
        {
            var bitsNumerator = capacity * Math.Abs(Math.Log(errorRate));
            var bitsDenominator = numSlices * Math.Pow(Math.Log(2), y: 2);
            return (int)Math.Ceiling(bitsNumerator / bitsDenominator);
        }
    }
}