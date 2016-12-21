using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace BloomFilters
{
    public class HashFunction
    {
        private readonly HashAlgorithm _algorithm;

        private List<byte> Input { get; } = new List<byte>();
        public byte[] Digest() => _algorithm.ComputeHash(Input.ToArray());

        public HashFunction(HashAlgorithm algorithm, IEnumerable<byte> input)
        {
            _algorithm = algorithm;
            Update(input);
        }

        public static Func<byte[], HashFunction> CreateHashFunction(HashAlgorithm algorithm)
        {
            return s => new HashFunction(algorithm, s);
        }

        public void Update(string key)
        {
            Update(key.GetBytes());
        }

        public void Update(IEnumerable<byte> input)
        {
            Input.AddRange(input);
        }

        public HashFunction Copy()
        {
            return new HashFunction(_algorithm, Input.ToList());
        }
    }
}