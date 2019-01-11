namespace Ocelot.Placeholders
{
    public interface IPlaceholderProvider
    {
        /// <summary>
        /// This value is used to match the provider to the method name defined in the template or other source
        /// </summary>
        string PlaceholderProviderName { get; }
    }
}
