namespace Ocelot.Placeholders
{
    public interface IPlaceholderFactory
    {
        IPlaceholderProvider Get(string placeHolder);
    }
}
