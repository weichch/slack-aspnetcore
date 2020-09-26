using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Represents middleware which receives event notification requests from Slack, verify them,
    /// and re-execute them in alternative request pipeline. This middleware does not handle request
    /// if response has started.
    /// </summary>
    public class SlackEventHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerFactory _loggerFactory;
        private readonly SlackEventHandlerOptions _options;
        private readonly ILogger _logger;
        private readonly LogLevel _logLevel;

        /// <summary>
        /// Creates an instance of the middleware.
        /// </summary>
        /// <param name="next">The next request handler in the request pipeline.</param>
        /// <param name="options">The middleware settings.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public SlackEventHandlerMiddleware(
            RequestDelegate next, 
            IOptions<SlackEventHandlerOptions> options,
            ILoggerFactory loggerFactory)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options.Value;
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<SlackEventHandlerMiddleware>();
            _logLevel = _options.InternalLogLevel;
        }

        /// <summary>
        /// If the request is from Slack, handles the request in event handlers.
        /// Otherwise, this method passes the request through.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        public Task Invoke(HttpContext httpContext)
        {
            if (_options.CallbackPath != httpContext.Request.Path)
            {
                return _next(httpContext);
            }
            
            if (httpContext.Response.HasStarted)
            {
                _logger.Log(_logLevel, "Unable to process request from Slack because response has started.");
                return _next(httpContext);
            }

            return HandleSlackEvent(httpContext);
        }

        /// <summary>
        /// Handles request sent to Slack callback path. This method verifies the request before
        /// calling registered event handlers to handle the request.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        private async Task HandleSlackEvent(HttpContext httpContext)
        {
            // Prepare HTTP context for Slack event handlers
            httpContext.Response.Clear();
            httpContext.Response.OnStarting(SetNoCacheHeaders, httpContext.Response);

            // Ensure HTTP method and content type
            if (!EnsureHttpMethodAndContentType(httpContext))
            {
                return;
            }

            var feature = ProvisionFeatures(httpContext);
            var request = httpContext.Request;
            request.EnableBuffering();

            // Verify the content is from Slack
            if (!await httpContext.VerifySlackRequestAsync())
            {
                _logger.Log(_logLevel, "Failed to verify a request from Slack.");
                return;
            }

            // Handle request in event handlers
            var eventContext = new SlackEventContext(httpContext, feature.EventAttributesProvider);
            await eventContext.FetchEventTypesAsync();

            SlackEventHandlerResult result = default;
            foreach (var eventHandler in _options.EventsHandlers)
            {
                try
                {
                    result = await eventHandler.HandleAsync(eventContext);
                    if (result.IsHandled)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    // If there is any error in the handler we skip the handler
                    // In the future we might be able to expose the exceptions
                    // to somewhere but it's out of scope for now
                    _logger.Log(_logLevel, ex, "Unhandled exception occurred in event handler.");
                }
            }

            if (result.IsHandled)
            {
                if (result.IsRedirect)
                {
                    HandleRedirect(httpContext, result);
                }
                else if (result.IsRewrite)
                {
                    await HandleRewrite(httpContext, result);
                }
            }
            else if (_options.FallbackResponseFactory != null)
            {
                // Handle by fallback response factory
                await _options.FallbackResponseFactory(httpContext);
            }
            else
            {
                // If the event request can't be handled, end the request with an error code
                _logger.Log(_logLevel, "Unable to handle a request from Slack.");
                httpContext.Response.StatusCode = StatusCodes.Status421MisdirectedRequest;
            }
        }

        /// <summary>
        /// Ensures the HTTP method and content type are allowed.
        /// </summary>
        private bool EnsureHttpMethodAndContentType(HttpContext httpContext)
        {
            var request = httpContext.Request;

            if (!_options.AllowedVerbs.Contains(request.Method))
            {
                httpContext.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                return false;
            }

            bool contentTypeAllowed;
            try
            {
                var contentType = new ContentType(request.ContentType);
                contentTypeAllowed = _options.AllowedContentTypes.Contains(contentType.MediaType);
            }
            catch
            {
                contentTypeAllowed = false;
            }

            if (!contentTypeAllowed)
            {
                httpContext.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets per-request services used by request validator and event handlers.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        private SlackEventHandlerFeature ProvisionFeatures(HttpContext httpContext)
        {
            var feature = new SlackEventHandlerFeature
            {
                LogLevel = _options.InternalLogLevel,
                SerializerOptions = _options.GetOrCreateJsonSerializerOptions(),
                RequestValidator = httpContext.GetSlackRequestValidator(),
                Parameters = _options.RequestValidationParameters
            };
            feature.EventAttributesProvider = CreateEventAttributesProvider(httpContext, feature);

            httpContext.Features.Set<ISlackEventHandlerServicesFeature>(feature);
            httpContext.Features.Set<ISlackRequestVerificationFeature>(feature);

            return feature;
        }

        /// <summary>
        /// Sets no cache headers to response.
        /// </summary>
        private static Task SetNoCacheHeaders(object state)
        {
            var headers = ((HttpResponse)state).Headers;
            headers[HeaderNames.CacheControl] = "no-cache";
            headers[HeaderNames.Pragma] = "no-cache";
            headers[HeaderNames.Expires] = "-1";
            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates <see cref="IEventAttributesProvider"/>.
        /// </summary>
        private IEventAttributesProvider CreateEventAttributesProvider(
            HttpContext httpContext,
            ISlackEventHandlerServicesFeature feature)
        {
            IEventAttributesProvider provider;

            if (_options.EventAttributesProvider != null)
            {
                provider = _options.EventAttributesProvider;
            }
            else if (_options.EventAttributesProviderType != null)
            {
                if (_options.EventAttributesProviderParameters?.Any() == true)
                {
                    provider = (IEventAttributesProvider)ActivatorUtilities.CreateInstance(
                        httpContext.RequestServices,
                        _options.EventAttributesProviderType,
                        _options.EventAttributesProviderParameters ?? Array.Empty<object>());
                }
                else
                {
                    provider = (IEventAttributesProvider) ActivatorUtilities.GetServiceOrCreateInstance(
                        httpContext.RequestServices,
                        _options.EventAttributesProviderType);
                }
            }
            else
            {
                // Create default event attributes provider
                provider = new EventAttributesProvider(feature, _loggerFactory);
            }

            return provider;
        }

        /// <summary>
        /// Handles redirect result.
        /// </summary>
        private void HandleRedirect(HttpContext httpContext, in SlackEventHandlerResult result)
        {
            httpContext.Response.StatusCode = _options.RedirectStatusCode;
            result.ApplyTo(httpContext);
        }

        /// <summary>
        /// Handles rewrite result.
        /// </summary>
        private Task HandleRewrite(HttpContext httpContext, in SlackEventHandlerResult result)
        {
            // Clear HTTP context for rewrite
            httpContext.SetEndpoint(null);
            httpContext.Features.Get<IRouteValuesFeature>()?.RouteValues?.Clear();

            result.ApplyTo(httpContext);

            return Awaited(httpContext, _next, _options.CallbackPath);

            static async Task Awaited(HttpContext httpContext, RequestDelegate next, PathString originalPath)
            {
                var request = httpContext.Request;
                try
                {
                    await next(httpContext);
                }
                finally
                {
                    request.Path = originalPath;
                }
            }
        }
    }
}
