using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        private readonly SlackEventHandlerOptions _options;

        /// <summary>
        /// Creates an instance of the middleware.
        /// </summary>
        /// <param name="next">The next request handler in the request pipeline.</param>
        /// <param name="options">The middleware settings.</param>
        public SlackEventHandlerMiddleware(
            RequestDelegate next, 
            IOptions<SlackEventHandlerOptions> options)
        {
            _next = next;
            _options = options.Value;
        }

        /// <summary>
        /// If the request is from Slack, handles the request in event handlers.
        /// Otherwise, this method passes the request through.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        public Task Invoke(HttpContext httpContext)
        {
            var request = httpContext.Request;
            if (_options.CallbackPath != request.Path
                || httpContext.Response.HasStarted
                || !_options.AllowedVerbs.Contains(request.Method))
            {
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
            // Sets services required by request validator and event handlers to feature 
            ProvisionFeatures(httpContext);

            // Buffer the request body
            var request = httpContext.Request;
            request.EnableBuffering();

            // Verify the content is from Slack
            if (!await httpContext.VerifySlackRequestAsync())
            {
                return;
            }
            
            // Handle request in event handlers
            var eventContext = new SlackEventContext(httpContext);
            foreach (var eventHandler in _options.EventsHandlers)
            {
                try
                {
                    await eventHandler.HandleAsync(eventContext);
                }
                catch
                {
                    // If there is any error in the handler we skip the handler
                    // In the future we might be able to expose the exceptions
                    // to somewhere but it's out of scope for now
                    eventContext.NoResult();
                }

                switch (eventContext.Result)
                {
                    case SlackEventHandling.None:
                        continue;

                    case SlackEventHandling.Redirect:
                        httpContext.Response.Clear();
                        HandleRedirect(httpContext, eventContext);
                        return;

                    case SlackEventHandling.Rewrite:
                        httpContext.Response.Clear();
                        await HandleRewrite(httpContext, eventContext);
                        return;

                    case SlackEventHandling.EndResponse:
                        return;
                }
            }

            // Handle event using fallback response factory
            if (_options.FallbackResponseFactory != null)
            {
                await _options.FallbackResponseFactory(httpContext);
                return;
            }

            // If the event request can't be handled, continue in next request handler
            // This may lead to an HTTP 404 error depends on if there is endpoint or not
            await _next(httpContext);
        }

        /// <summary>
        /// Sets per-request services used by request validator and event handlers.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        private void ProvisionFeatures(HttpContext httpContext)
        {
            var jsonSerializerOptions =
                _options.DefaultSerializerOptions
                ?? httpContext.RequestServices
                    .GetRequiredService<IOptions<JsonOptions>>().Value
                    .JsonSerializerOptions;

            var feature = new SlackEventHandlerFeature
            {
                SerializerOptions = jsonSerializerOptions,
                RequestValidator = httpContext.GetSlackRequestValidator(),
                Parameters = _options.RequestValidationParameters
            };
            feature.AttributesReader = CreateEventAttributesReader(httpContext, feature);

            httpContext.Features.Set<ISlackEventHandlerServicesFeature>(feature);
        }

        /// <summary>
        /// Creates <see cref="IEventAttributesReader"/>.
        /// </summary>
        private IEventAttributesReader CreateEventAttributesReader(
            HttpContext httpContext, 
            ISlackEventHandlerServicesFeature feature)
        {
            IEventAttributesReader reader;

            if (_options.EventAttributesReader != null)
            {
                reader = _options.EventAttributesReader;
            }
            else if (_options.EventAttributesReaderType != null)
            {
                if (_options.EventAttributesReaderParameters?.Any() == true)
                {
                    reader = (IEventAttributesReader)ActivatorUtilities.CreateInstance(
                        httpContext.RequestServices,
                        _options.EventAttributesReaderType,
                        _options.EventAttributesReaderParameters ?? Array.Empty<object>());
                }
                else
                {
                    reader = (IEventAttributesReader) ActivatorUtilities.GetServiceOrCreateInstance(
                        httpContext.RequestServices,
                        _options.EventAttributesReaderType);
                }
            }
            else
            {
                // Create default event attributes reader
                reader = new DefaultEventAttributesReader(feature);
            }

            return reader;
        }

        /// <summary>
        /// Handles redirect result.
        /// </summary>
        private void HandleRedirect(HttpContext httpContext, SlackEventContext eventContext)
        {
            httpContext.Response.StatusCode = _options.RedirectStatusCode;

        }

        private async Task HandleRewrite(HttpContext httpContext, SlackEventContext eventContext)
        {

        }
    }
}
