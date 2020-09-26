using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RabbitSharp.Slack.Events.Models;

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
        /// Gets or sets the type of the event dispatch.
        /// </summary>
        public string? EventDispatchType { get; private set; }

        /// <summary>
        /// Gets or sets the type of the event data.
        /// </summary>
        public string? EventType { get; private set; }

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
        /// Sets <see cref="EventDispatchType"/> and <see cref="EventType"/>.
        /// </summary>
        public async ValueTask FetchEventTypesAsync()
        {
            if (!string.IsNullOrWhiteSpace(EventDispatchType))
            {
                return;
            }

            var requestBody = HttpContext.Request.Body;
            if (requestBody.CanSeek)
            {
                requestBody.Seek(0, SeekOrigin.Begin);
            }

            using var jsonDoc = await JsonDocument.ParseAsync(
                requestBody,
                cancellationToken: HttpContext.RequestAborted);

            jsonDoc.ReadEventTypes(out var eventDispatchType, out var eventType);
            EventDispatchType = eventDispatchType;
            EventType = eventType;
        }

        /// <summary>
        /// Constructs event attributes.
        /// </summary>
        public async ValueTask FetchEventAttributesAsync()
        {
            if (EventAttributes != null)
            {
                return;
            }

            var providerContext = new EventAttributesProviderContext(this);
            EventAttributes = await EventAttributesProvider.GetEventAttributes(providerContext);
        }
    }
}
