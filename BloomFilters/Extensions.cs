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

        public static string GetString(this byte[] bytes)
        {
            var chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, srcOffset: 0, dst: chars, dstOffset: 0, count: bytes.Length);
            return new string(chars);
        }
    }
}