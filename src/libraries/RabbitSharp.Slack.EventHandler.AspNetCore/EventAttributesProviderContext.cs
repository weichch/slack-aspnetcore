using System;
using System.IO;
using System.Threading;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Represents data context for <see cref="IEventAttributesProvider"/>.
    /// </summary>
    public class EventAttributesProviderContext
    {
        /// <summary>
        /// Creates an instance of the context.
        /// </summary>
        /// <param name="eventContext">The event context.</param>
        public EventAttributesProviderContext(SlackEventContext eventContext)
        {
            EventContext = eventContext ?? throw new ArgumentNullException(nameof(eventContext));

            var httpContext = eventContext.HttpContext;
            Event = httpContext.Request.Body;
            CancellationToken = httpContext.RequestAborted;
        }

        /// <summary>
        /// Gets the event context.
        /// </summary>
        public SlackEventContext EventContext { get; }

        /// <summary>
        /// Gets the stream which contains event data.
        /// </summary>
        public Stream Event { get; }

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        public CancellationToken CancellationToken { get; }
    }
}
