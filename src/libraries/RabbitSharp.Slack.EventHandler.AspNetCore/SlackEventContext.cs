using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Represents the data context for event handlers. Event handler should set result
    /// after it has completed processing the event.
    /// </summary>
    public class SlackEventContext
    {
        private LinkGenerator? _linkGenerator;
        private EventAttributesReaderContext? _readerContext;

        /// <summary>
        /// Creates an instance of the event context.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="eventAttributesReader">The event attributes reader.</param>
        public SlackEventContext(HttpContext httpContext, IEventAttributesReader eventAttributesReader)
        {
            HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            EventAttributes = eventAttributesReader ?? throw new ArgumentNullException(nameof(eventAttributesReader));
        }

        /// <summary>
        /// Gets the HTTP context.
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// Gets the event attributes reader.
        /// </summary>
        public IEventAttributesReader EventAttributes { get; }

        /// <summary>
        /// Gets an instance of <see cref="LinkGenerator"/> for composing URLs. You will need to
        /// enable routing to be able to use this property.
        /// </summary>
        public LinkGenerator LinkHelper => _linkGenerator
            ??= HttpContext.RequestServices.GetRequiredService<LinkGenerator>();

        /// <summary>
        /// Reads event attributes.
        /// </summary>
        /// <typeparam name="TAttributes">The type of the event attributes.</typeparam>
        public ValueTask<TAttributes> ReadEventAttributes<TAttributes>()
        {
            if (_readerContext == null)
            {
                _readerContext = new EventAttributesReaderContext(HttpContext);
            }

            return EventAttributes.ReadAsync<TAttributes>(_readerContext);
        }
    }
}
