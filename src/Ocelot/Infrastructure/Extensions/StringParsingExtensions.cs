namespace Ocelot.Infrastructure.Extensions
{
    using System.Text.RegularExpressions;

    public static class StringParsingExtensions
    {
        private static readonly Regex _placeholderPatternMatcher = new Regex(@"{([^}(->)]*)(->)?([^}]*)?}", 
            RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>
        /// Runs an optimized regex against the specified template.
        /// </summary>
        /// <param name="template">The template to find placeholders in.</param>
        /// <returns>
        /// The MatchCollection with all the placeholders found in the template string. Each match will have three
        /// capturing groups with the key, operator, and value respectively. Operator will be zero length if the
        /// operator and value are not found.
        /// </returns>
        public static MatchCollection MatchPlaceholders(this string template)
        {
            return _placeholderPatternMatcher.Matches(template ?? string.Empty);
        }

        /// <summary>
        /// Returns true if the specified template has any placeholders
        /// </summary>
        /// <param name="template">The template to check for placeholders</param>
        /// <returns>Boolean indicating if the template has placeholders</returns>
        public static bool HasPlaceholders(this string template)
        {
            return _placeholderPatternMatcher.Match(template ?? string.Empty).Success;
        }
    }
}
