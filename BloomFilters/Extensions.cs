using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BloomFilters
{
    public static class Extensions
    {
        public static byte[] GetBytes(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static string GetString(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static string GetHexString(this byte[] ba)
        {
            return ToFormattedString(ba, b => $"{b:X2}");
        }

        public static string GetDecimalString(this byte[] ba)
        {
            return ToFormattedString(ba, b => $"{b:D}");
        }

        private static string ToFormattedString(IReadOnlyCollection<byte> ba, Func<byte, string> provider)
        {
            var hex = new StringBuilder(ba.Count * 2);

            foreach (var b in ba)
            {
                hex.AppendFormat(provider(b));
            }

            return hex.ToString();
        }

        public static byte[] GetBytesFromHexString(this string hex)
        {
            return Enumerable.Range(start: 0, count: hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, length: 2), fromBase: 16))
                             .ToArray();
        }

        public static IEnumerable<uint> UnpackToUInt(this byte[] bytes)
        {
            const int size = sizeof(uint);

            return bytes.Select((b, i) => (@byte: b, index: i))
                        .GroupBy(tuple => tuple.index / size)
                        .Where(group => group.Count() == size)
                        .Select(group => ToUInt32(group.Select(t => t.@byte)));
        }

        private static uint ToUInt32(IEnumerable<byte> bytes)
        {
            return BitConverter.ToUInt32(bytes.ToArray(), startIndex: 0);
        }

        public static List<T> Combine<T>(this IEnumerable<T> t1, IEnumerable<T> t2)
        {
            var list = new List<T>(t1);
            list.AddRange(t2);
            return list;
        }
    }
}