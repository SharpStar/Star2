using System.Text.RegularExpressions;

namespace Star.Common.Extensions
{
    public static class StringExtensions
    {
        private static readonly Regex StripColorRegex = new Regex("\\^[#a-zA-Z0-9]+;", RegexOptions.Compiled);

        public static string StripStarColors(this string str)
        {
            return StripColorRegex.Replace(str, string.Empty);
        }
    }
}
