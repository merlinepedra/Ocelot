namespace Ocelot.Infrastructure.Extensions
{
    using System.Text.RegularExpressions;

    public static class StringParsingExtensions
    {
        private static readonly Regex _placeholderPatternMatcher = new Regex(@"{[^}]*}", RegexOptions.Compiled | RegexOptions.Singleline);

        public static MatchCollection MatchPlaceholders(this string template)
        {
            return _placeholderPatternMatcher.Matches(template);
        }

        public static bool HasPlaceholders(this string template)
        {
            return _placeholderPatternMatcher.Match(template ?? string.Empty).Success;
        }
    }
}
