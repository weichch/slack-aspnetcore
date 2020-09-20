using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Represents settings for handling Slack events.
    /// </summary>
    public class SlackEventHandlerOptions
    {
        /// <summary>
        /// Creates an instance of the options.
        /// </summary>
        public SlackEventHandlerOptions()
        {
            EventsHandlers = new List<ISlackEventHandler>();

            // TODO I wonder if 307 or 308 is more appropriate than 302, but Slack docs suggest
            // TODO 301 or 302 https://api.slack.com/events-api#the-events-api__field-guide__error-handling
            RedirectStatusCode = StatusCodes.Status302Found;
            AllowedVerbs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                HttpMethods.Post
            };
        }

        /// <summary>
        /// Gets or sets the path within the base path of the application where the Slack event
        /// notifications will be received. Requests sent to this path from Slack APIs will be
        /// handled by the middleware. The fully qualified URL (when this path is combined with
        /// other components of the request URL) should match the Request URL configured in
        /// Slack API portal.
        /// </summary>
        public PathString CallbackPath { get; set; }

        /// <summary>
        /// Gets a list of allowed HTTP verbs. By default, only HTTP Post requests are allowed.
        /// </summary>
        public ISet<string> AllowedVerbs { get; set; }

        /// <summary>
        /// Gets or sets the default settings for <see cref="System.Text.Json.JsonSerializer"/> used by
        /// built-in types. If this property is <c>null</c>, <see cref="JsonOptions.JsonSerializerOptions"/>
        /// is used.
        /// </summary>
        public JsonSerializerOptions? DefaultSerializerOptions { get; set; }

        /// <summary>
        /// Gets a list of registered event handlers used by the middleware to handle Slack events.
        /// </summary>
        public IList<ISlackEventHandler> EventsHandlers { get; }

        /// <summary>
        /// Gets or sets an instance of <see cref="IEventAttributesReader"/> used by the middleware
        /// to fetch attributes for each event received.
        /// </summary>
        public IEventAttributesReader? EventAttributesReader { get; set; }

        /// <summary>
        /// Gets or sets the type of <see cref="IEventAttributesReader"/> used by the middleware to
        /// fetch attributes for each event received. When this property is set and <see cref="EventAttributesReader"/>
        /// is not set, the type is activated using dependency injection in the middleware. 
        /// </summary>
        public Type? EventAttributesReaderType { get; set; }

        /// <summary>
        /// Gets or sets the parameters to pass into the constructor while activating <see cref="EventAttributesReaderType"/>.
        /// </summary>
        public object[]? EventAttributesReaderParameters { get; set; }

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
        public int RedirectStatusCode { get; }
    }
}
