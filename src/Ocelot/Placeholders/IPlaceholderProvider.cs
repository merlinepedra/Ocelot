namespace Ocelot.Placeholders
{
    using System.Collections.Generic;
    using Ocelot.Middleware;

    public interface IPlaceholderProvider
    {
        /// <summary>
        /// Gets the value that is used to match the provider to the method name defined in the template or other source
        /// </summary>
        string PlaceholderProviderName { get; }

        IEnumerable<string> GetValues(DownstreamContext context, string value);
    }
}
