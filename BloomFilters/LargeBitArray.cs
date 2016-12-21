using System;

namespace BloomFilters
{
    public class LargeBitArray
    {
        private byte[] BackingArray { get; }

        public LargeBitArray(long size)
        {
            if (size >= int.MaxValue * 8L)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "too big");
            }
            BackingArray = new byte[size / 8];
        }

        public void Set(long number)
        {
            var bucket = (int)(number / byte.MaxValue);
            var bit = (byte)(number % byte.MaxValue);
            BackingArray[bucket] |= bit;
        }

        public bool Get(long index)
        {
            var bucket = (int)(index / byte.MaxValue);
            var bit = (byte)(index % byte.MaxValue);
            return (BackingArray[bucket] & bit) != 0;
        }
    }
}