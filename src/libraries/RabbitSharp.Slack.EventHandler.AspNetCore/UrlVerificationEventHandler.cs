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
    public class UrlVerificationEventHandler : ISlackEventHandler
    {
        /// <summary>
        /// Verifies the request is from Slack and is of <c>url_verification</c> type, then
        /// responds to the challenge.
        /// </summary>
        /// <param name="context">The event context.</param>
        public async ValueTask HandleAsync(SlackEventContext context)
        {
            var httpContext = context.HttpContext;
            var request = httpContext.Request;

            // Verify the content type is JSON
            if (!string.Equals(
                request.ContentType,
                ContentTypeApplicationJson,
                StringComparison.OrdinalIgnoreCase))
            {
                context.NoResult();
                return;
            }

            // Read event content
            var content = await context.ReadEventAttributes<UrlVerification?>();
            if (content == null)
            {
                context.NoResult();
                return;
            }

            // Verify the event type
            if (!string.Equals(
                content.Type,
                EventTypeUrlVerification,
                StringComparison.OrdinalIgnoreCase))
            {
                context.NoResult();
                return;
            }

            httpContext.Response.Clear();
            httpContext.Response.StatusCode = StatusCodes.Status200OK;
            httpContext.Response.ContentType = ContentTypePlainText;
            await httpContext.Response.WriteAsync(content.Challenge);
            context.EndResponse();
        }
    }
}
