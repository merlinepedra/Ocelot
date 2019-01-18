using Ocelot.Infrastructure.Extensions;

namespace Ocelot.Values
{
    public class DownstreamPathTemplate
    {
        public DownstreamPathTemplate(string value)
        {
            Value = value;
            HasPlaceholders = value.HasPlaceholders();
        }

        public string Value { get; }
        
        public bool HasPlaceholders { get; }
    }
}
