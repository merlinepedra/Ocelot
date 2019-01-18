namespace Ocelot.Placeholders.Providers
{
    using System;
    using System.Collections.Generic;
    using Middleware;

    public class EnvironmentProvider : IPlaceholderProvider
    {
        public string PlaceholderProviderName { get; } = "env";

        public IEnumerable<string> GetValues(DownstreamContext context, string value)
        {
            return new[] { Environment.GetEnvironmentVariable(value) };
        }
    }
}
