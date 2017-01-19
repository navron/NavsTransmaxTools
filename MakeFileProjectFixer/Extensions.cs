using System;

namespace MakeFileProjectFixer
{
    public static class StringExtensions
    {
        public static bool Contains(this string source, string contains, StringComparison stringComparison)
        {
            return source.IndexOf(contains, stringComparison) >= 0;
        }
    }
}
