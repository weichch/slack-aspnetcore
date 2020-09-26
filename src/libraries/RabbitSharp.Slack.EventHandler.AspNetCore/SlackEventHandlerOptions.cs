using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitSharp.Slack.Events.Models;
using RabbitSharp.Slack.Http;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Represents settings for handling Slack events.
    /// </summary>
    public class SlackEventHandlerOptions
    {
        private readonly IApplicationBuilder? _applicationBuilder;

        /// <summary>
        /// Creates an instance of the options.
        /// </summary>
        public SlackEventHandlerOptions()
            : this(null)
        {
        }

        /// <summary>
        /// Creates an instance of the options.
        /// </summary>
        public SlackEventHandlerOptions(IApplicationBuilder? app)
        {
            _applicationBuilder = app;
            CallbackPath = "/slack/events";
            EventsHandlers = new List<ISlackEventHandler>();

            // TODO I wonder if 307 or 308 is more appropriate than 302, but Slack docs suggest
            // TODO 301 or 302 https://api.slack.com/events-api#the-events-api__field-guide__error-handling
            RedirectStatusCode = StatusCodes.Status302Found;
            AllowedVerbs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                HttpMethods.Post
            };
            AllowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                SlackEventHandlerConstants.ContentTypeApplicationJson
            };
            InternalLogLevel = LogLevel.Trace;
        }

        /// <summary>
        /// Gets the application builder.
        /// </summary>
        public IApplicationBuilder ApplicationBuilder
        {
            get
            {
                if (_applicationBuilder == null)
                {
                    throw new InvalidOperationException(
                        "No application builder set. Use 'SlackEventHandlerOptions(IApplicationBuilder)' constructor.");
                }

                return _applicationBuilder;
            }
        }

        /// <summary>
        /// Gets or sets the path within the base path of the application where the Slack event
        /// notifications will be received. Requests sent to this path from Slack APIs will be
        /// handled by the middleware. The fully qualified URL (when this path is combined with
        /// other components of the request URL) should match the Request URL configured in
        /// Slack API portal. Default value is <c>/slack/events</c>.
        /// </summary>
        public PathString CallbackPath { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="LogLevel"/> used by the internal logger. Default value is
        /// <see cref="LogLevel.Trace"/>.
        /// </summary>
        public LogLevel InternalLogLevel { get; set; }

        /// <summary>
        /// Gets a list of allowed HTTP verbs. By default, only HTTP POST requests are allowed.
        /// </summary>
        public ISet<string> AllowedVerbs { get; }

        /// <summary>
        /// Gets a list of allowed <c>Content-Type</c> values. By default, only <c>application/json</c>
        /// is allowed.
        /// </summary>
        public ISet<string> AllowedContentTypes { get; }

        /// <summary>
        /// Gets or sets the default settings for <see cref="System.Text.Json.JsonSerializer"/> used by
        /// built-in types. If this property is <c>null</c>, <see cref="JsonOptions.JsonSerializerOptions"/>
        /// is used.
        /// </summary>
        public JsonSerializerOptions? DefaultSerializerOptions { get; set; }

        /// <summary>
        /// Gets or sets whether to deserialize additional properties as data extensions
        /// when constructing <see cref="EventWrapper"/> and <see cref="EventProperties"/>.
        /// When this property is set to <c>true</c>, properties not defined in model types
        /// will be deserialized into extensions property. Default value is <c>false</c>.
        /// </summary>
        public bool DeserializeAdditionalProperties { get; set; }

        /// <summary>
        /// Gets a list of registered event handlers used by the middleware to handle Slack events.
        /// </summary>
        public IList<ISlackEventHandler> EventsHandlers { get; }

        /// <summary>
        /// Gets or sets an instance of <see cref="IEventAttributesProvider"/> used by the middleware
        /// to fetch attributes for each event received.
        /// </summary>
        public IEventAttributesProvider? EventAttributesProvider { get; set; }

        /// <summary>
        /// Gets or sets the type of <see cref="IEventAttributesProvider"/> used by the middleware to
        /// fetch attributes for each event received. When this property is set and <see cref="EventAttributesProvider"/>
        /// is not set, the type is activated using dependency injection in the middleware. 
        /// </summary>
        public Type? EventAttributesProviderType { get; set; }

        /// <summary>
        /// Gets or sets the parameters to pass into the constructor while activating <see cref="EventAttributesProviderType"/>.
        /// </summary>
        public object[]? EventAttributesProviderParameters { get; set; }

        /// <summary>
        /// Gets or sets the fallback response factory which is invoked when no event handler can handle
        /// received event. When this property is not provided, the request is passed to next request handler
        /// in the request pipeline.
        /// </summary>
        public RequestDelegate? FallbackResponseFactory { get; set; }

        /// <summary>
        /// Gets or sets parameters used to verify requests from Slack. If this property
        /// is <c>null</c>, requests will not be verified.
        /// </summary>
        public SlackRequestValidationParameters? RequestValidationParameters { get; set; }

        /// <summary>
        /// Gets or sets the status code used when redirecting the response. Default value is <c>302 Found</c>.
        /// </summary>
        public int RedirectStatusCode { get; set; }

        /// <summary>
        /// If <see cref="DefaultSerializerOptions"/> is not <c>null</c>, returns <see cref="DefaultSerializerOptions"/>.
        /// Otherwise, creates an instance of <see cref="JsonSerializerOptions"/> with a set of default settings.
        /// </summary>
        public JsonSerializerOptions GetOrCreateJsonSerializerOptions()
        {
            if (DefaultSerializerOptions != null)
            {
                return DefaultSerializerOptions;
            }

            return new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Converters =
                {
                    new EventWrapperJsonConverter {SupportExtensions = DeserializeAdditionalProperties},
                    new EventPropertiesJsonConverter {SupportExtensions = DeserializeAdditionalProperties}
                }
            };
        }

    }
}
