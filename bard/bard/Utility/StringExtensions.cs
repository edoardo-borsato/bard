using System.Text;

namespace bard.Utility;

internal static class StringExtensions
{
    public static MemoryStream ToStream(this string str)
    {
        return new MemoryStream(Encoding.Default.GetBytes(str));
    }
}