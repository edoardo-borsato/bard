using System.Text;

namespace bard.Utility
{
    internal static class StringExtensions
    {
        public static Stream ToStream(this string str)
        {
            return new MemoryStream(Encoding.Default.GetBytes(str));
        }
    }
}
