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

        /// <summary>
        /// Creates an instance of the event context.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="eventAttributesProvider">The event attributes provider.</param>
        public SlackEventContext(HttpContext httpContext, IEventAttributesProvider eventAttributesProvider)
        {
            HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            EventAttributesProvider = eventAttributesProvider ?? throw new ArgumentNullException(nameof(eventAttributesProvider));
        }

        /// <summary>
        /// Gets the HTTP context.
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// Gets the event attributes provider.
        /// </summary>
        public IEventAttributesProvider EventAttributesProvider { get; }

        /// <summary>
        /// Gets the event attributes.
        /// </summary>
        public object? EventAttributes { get; private set; }

        /// <summary>
        /// Gets an instance of <see cref="LinkGenerator"/> for composing URLs. You will need to
        /// enable routing to be able to use this property.
        /// </summary>
        public LinkGenerator LinkHelper => _linkGenerator
            ??= HttpContext.RequestServices.GetRequiredService<LinkGenerator>();

        /// <summary>
        /// Constructs event attributes.
        /// </summary>
        public async ValueTask FetchEventAttributesAsync()
        {
            if (EventAttributes != null)
            {
                return;
            }

            var providerContext = new EventAttributesProviderContext(HttpContext);
            EventAttributes = await EventAttributesProvider.GetEventAttributes(providerContext);
        }
    }
}
