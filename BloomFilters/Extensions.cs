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
            var hex = new StringBuilder(ba.Length * 2);

            foreach (var b in ba)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }
    }
}