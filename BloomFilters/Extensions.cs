using System;

namespace BloomFilters
{
    public static class Extensions
    {
        public static byte[] GetBytes(this string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), srcOffset: 0, dst: bytes, dstOffset: 0, count: bytes.Length);
            return bytes;
        }
    }
}