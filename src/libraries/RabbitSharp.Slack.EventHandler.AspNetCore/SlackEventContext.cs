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
        private string? _eventType;

        /// <summary>
        /// Creates an instance of the event context.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="eventData">The event data.</param>
        /// <param name="eventAttributesProvider">The event attributes provider.</param>
        public SlackEventContext(HttpContext httpContext, JsonDocument eventData, IEventAttributesProvider eventAttributesProvider)
        {
            HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            EventAttributesProvider = eventAttributesProvider ?? throw new ArgumentNullException(nameof(eventAttributesProvider));
            Event = eventData ?? throw new ArgumentNullException(nameof(eventData));
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
        /// Gets or sets the type of the event. If event is wrapped in <see cref="EventWrapper"/>,
        /// this property returns the type of the wrapped event. Otherwise, this property returns
        /// <see cref="EventDispatchType"/>.
        /// </summary>
        public string? EventType
        {
            get => _eventType ?? EventDispatchType;
            private set => _eventType = value;
        }

        /// <summary>
        /// Gets the event attributes.
        /// </summary>
        public object? EventAttributes { get; private set; }

        /// <summary>
        /// Gets the event object.
        /// </summary>
        public JsonDocument Event { get; }

        /// <summary>
        /// Gets an instance of <see cref="LinkGenerator"/> for composing URLs. You will need to
        /// enable routing to be able to use this property.
        /// </summary>
        public LinkGenerator LinkHelper => _linkGenerator
            ??= HttpContext.RequestServices.GetRequiredService<LinkGenerator>();

        /// <summary>
        /// Sets <see cref="EventDispatchType"/> and <see cref="EventType"/>.
        /// </summary>
        public void FetchEventTypes()
        {
            if (!string.IsNullOrWhiteSpace(EventDispatchType))
            {
                return;
            }

            Event.ReadEventTypes(out var eventDispatchType, out var eventType);
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

        /// <summary>
        /// Returns whether event type is equal to specified value.
        /// </summary>
        /// <param name="type">The event type.</param>
        public bool EventTypeEquals(string type)
        {
            FetchEventTypes();
            return string.Equals(EventType, type, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates an instance of <see cref="SlackEventContext"/> from HTTP request.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        public static SlackEventContext CreateFromHttpContext(HttpContext httpContext)
        {
            var feature = httpContext.Features.Get<ISlackEventHandlerServicesFeature>();
            if (feature == null)
            {
                throw new InvalidOperationException(
                    "No feature found. Have you added SlackEventHandlerMiddleware?");
            }

            var requestBody = httpContext.Request.Body;
            if (requestBody.CanSeek)
            {
                requestBody.Seek(0, SeekOrigin.Begin);
            }

            var eventObj = JsonDocument.Parse(requestBody);
            httpContext.Response.RegisterForDispose(eventObj);

            return new SlackEventContext(httpContext, eventObj, feature.EventAttributesProvider);
        }
    }
}
