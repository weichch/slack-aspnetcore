using System;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Http;

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
        /// <param name="httpContext">The HTTP context.</param>
        public EventAttributesProviderContext(HttpContext httpContext)
        {
            HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            Event = httpContext.Request.Body;
            CancellationToken = httpContext.RequestAborted;
        }

        /// <summary>
        /// Gets the HTTP context.
        /// </summary>
        public HttpContext HttpContext { get; }

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
