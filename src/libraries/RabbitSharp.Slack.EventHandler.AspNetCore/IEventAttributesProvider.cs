using System.Threading.Tasks;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Provides mechanism to inspect event payload and constructs event attributes
    /// of appropriate type.
    /// </summary>
    public interface IEventAttributesProvider
    {
        /// <summary>
        /// Constructs event specific attributes object.
        /// </summary>
        /// <param name="context">The data context of the provider.</param>
        ValueTask<object?> GetEventAttributes(EventAttributesProviderContext context);
    }
}