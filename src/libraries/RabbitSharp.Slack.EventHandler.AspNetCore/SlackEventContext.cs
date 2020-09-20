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
        private RoutePatternFormatter? _routePatternFormatter;
        private EventAttributesReaderContext? _readerContext;

        /// <summary>
        /// Creates an instance of the event context.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        public SlackEventContext(HttpContext httpContext)
            : this(httpContext, httpContext.GetSlackEventHandlerServicesFeature().AttributesReader)
        {
        }

        /// <summary>
        /// Creates an instance of the event context.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="eventAttributesReader">The event attributes reader.</param>
        public SlackEventContext(HttpContext httpContext, IEventAttributesReader eventAttributesReader)
        {
            HttpContext = httpContext;
            EventAttributes = eventAttributesReader;
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
        /// Gets the event handling result.
        /// </summary>
        public SlackEventHandling Result { get; private set; }

        /// <summary>
        /// When <see cref="Result"/> is <see cref="SlackEventHandling.Redirect"/>, this is
        /// the absolute location to be set in the HTTP header. When <see cref="Result"/> is
        /// <see cref="SlackEventHandling.Rewrite"/>, this is the path to the destination endpoint
        /// within the base path of the application.
        /// </summary>
        public string? Location { get; private set; }

        /// <summary>
        /// When <see cref="Result"/> is <see cref="SlackEventHandling.Redirect"/>, this is
        /// the HTTP status of the response. If this property is <c>null</c>,
        /// <see cref="SlackEventHandlerOptions.RedirectStatusCode"/> is used.
        /// </summary>
        public int? RedirectStatusCode { get; private set; }

        /// <summary>
        /// Gets an instance of <see cref="LinkGenerator"/> for composing URLs. You will need to
        /// enable routing to be able to use this property.
        /// </summary>
        public LinkGenerator LinkHelper => _linkGenerator
            ??= HttpContext.RequestServices.GetRequiredService<LinkGenerator>();

        /// <summary>
        /// Gets an instance of <see cref="RoutePatternFormatter"/> for formatting route templates.
        /// </summary>
        public RoutePatternFormatter RoutePatternHelper => _routePatternFormatter
            ??= ActivatorUtilities.CreateInstance<RoutePatternFormatter>(HttpContext.RequestServices);

        /// <summary>
        /// Sets <see cref="Result"/> to <see cref="SlackEventHandling.None"/>.
        /// </summary>
        public void NoResult()
        {
            Result = SlackEventHandling.None;
        }

        /// <summary>
        /// Sets <see cref="Result"/> to <see cref="SlackEventHandling.Redirect"/>.
        /// </summary>
        /// <param name="location">The absolute URI to the destination endpoint.</param>
        public void Redirect(Uri location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (!location.IsAbsoluteUri)
            {
                throw new ArgumentException("Location must be an absolute URI.");
            }

            Result = SlackEventHandling.Redirect;
            Location = location.AbsoluteUri;
        }

        /// <summary>
        /// Sets <see cref="Result"/> to <see cref="SlackEventHandling.Redirect"/>.
        /// </summary>
        /// <param name="statusCode">The status code of the HTTP response.</param>
        /// <param name="location">The absolute URI to the destination endpoint.</param>
        public void Redirect(int statusCode, Uri location)
        {
            Redirect(location);
            RedirectStatusCode = statusCode;
        }

        /// <summary>
        /// Sets <see cref="Result"/> to <see cref="SlackEventHandling.Rewrite"/>.
        /// </summary>
        /// <param name="path">The path within the base path of the application where the request is sent to.</param>
        public void Rewrite(PathString path)
        {
            Result = SlackEventHandling.Rewrite;
            Location = path;
        }

        /// <summary>
        /// Sets <see cref="Result"/> to <see cref="SlackEventHandling.EndResponse"/>.
        /// </summary>
        public void EndResponse()
        {
            Result = SlackEventHandling.EndResponse;
        }

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
