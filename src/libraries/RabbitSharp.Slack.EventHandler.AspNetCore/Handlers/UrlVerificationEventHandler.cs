using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using static RabbitSharp.Slack.Events.SlackEventHandlerConstants;

namespace RabbitSharp.Slack.Events.Handlers
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

            var urlVerification = await context.GetUrlVerificationAttributes();
            if (urlVerification is null)
            {
                return SlackEventHandlerResult.NoResult();
            }

            var httpContext = context.HttpContext;
            httpContext.Response.StatusCode = StatusCodes.Status200OK;
            httpContext.Response.ContentType = ContentTypePlainText;
            await httpContext.Response.WriteAsync(urlVerification.Challenge);
            return SlackEventHandlerResult.EndResponse();
        }
    }
}
