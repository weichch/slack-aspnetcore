using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RabbitSharp.Slack.Events.Models;
using static RabbitSharp.Slack.Events.SlackEventHandlerConstants;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Handles URL verification handshake according to Slack API documentation at
    /// https://api.slack.com/events-api#the-events-api__subscribing-to-event-types__events-api-request-urls__request-url-configuration--verification.
    /// </summary>
    class UrlVerificationEventHandler : ISlackEventHandler
    {
        public async ValueTask<SlackEventHandlerResult> HandleAsync(SlackEventContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var content = await context.ReadEventAttributes<UrlVerification?>();
            if (content == null
                || !string.Equals(content.Type, EventTypeUrlVerification, StringComparison.OrdinalIgnoreCase))
            {
                return SlackEventHandlerResult.NoResult();
            }

            var httpContext = context.HttpContext;
            httpContext.Response.StatusCode = StatusCodes.Status200OK;
            httpContext.Response.ContentType = ContentTypePlainText;
            await httpContext.Response.WriteAsync(content.Challenge);
            return SlackEventHandlerResult.EndResponse();
        }
    }
}
