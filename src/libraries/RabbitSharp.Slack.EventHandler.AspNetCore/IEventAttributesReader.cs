using System.Threading.Tasks;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Provides mechanism for event handlers to fetch event specific attributes.
    /// </summary>
    public interface IEventAttributesReader
    {
        /// <summary>
        /// Constructs an object of <typeparamref name="T"/> which contains event specific attributes.
        /// </summary>
        /// <typeparam name="T">The type of the event attributes object.</typeparam>
        /// <param name="context">The data context of the reader.</param>
        ValueTask<T> ReadAsync<T>(EventAttributesReaderContext context);
    }
}