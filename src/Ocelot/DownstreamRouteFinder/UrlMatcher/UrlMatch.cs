using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace Ocelot.DownstreamRouteFinder.UrlMatcher
{
    public class UrlMatch
    {
        public UrlMatch(Match match)
        {
            Match = match; 
        }

        public Match Match { get; }
        
        public bool IsMatch => Match != null && Match.Success;
    }
}
