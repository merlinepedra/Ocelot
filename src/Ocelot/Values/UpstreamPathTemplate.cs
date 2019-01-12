using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Ocelot.Values
{
    public class UpstreamPathTemplate
    {
        public UpstreamPathTemplate(string template, int priority, bool containsQueryString, string originalValue, List<string> keys)
        {
            Priority = priority;
            ContainsQueryString = containsQueryString;
            OriginalValue = originalValue;
            Pattern = template == null ? 
                new Regex("$^", RegexOptions.Compiled | RegexOptions.Singleline) : 
                new Regex(template, RegexOptions.Compiled | RegexOptions.Singleline);
            Keys = keys.ToImmutableHashSet();
        }

        public int Priority { get; }

        public bool ContainsQueryString { get; }

        public string OriginalValue { get; }
        
        public Regex Pattern { get; }
        
        public ImmutableHashSet<string> Keys { get; }
    }
}
