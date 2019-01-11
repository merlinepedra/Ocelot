using System;
using Ocelot.Infrastructure.Extensions;
using Ocelot.Placeholders.Providers;

namespace Ocelot.Placeholders
{
    public class PlaceholderFactory : IPlaceholderFactory
    {
        public IPlaceholderProvider Get(string placeHolder)
        {
            //TODO: These should be received from DI instead of hardcoded.
            
            //Return default if not a methodized template
            if (!placeHolder.Contains("->"))
                return new DefaultPlaceholderProvider();
            
            //For all others...
            var placeholderProviderName = placeHolder.Split("->", 2)[0].Trim('{');
            switch (placeholderProviderName)
            {
                //If all else fails, maybe it's just a value from upstream?
                default:
                    return new DefaultPlaceholderProvider();
            }
        }
    }
}
