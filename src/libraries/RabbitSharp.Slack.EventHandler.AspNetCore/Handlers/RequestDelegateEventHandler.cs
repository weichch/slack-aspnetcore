using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RabbitSharp.Slack.Events.Handlers
{
    /// <summary>
    /// Implements <see cref="ISlackEventHandler"/> by using a <see cref="RequestDelegate"/> to handle event.
    /// </summary>
    class RequestDelegateEventHandler : ISlackEventHandler
    {
        private readonly Predicate<SlackEventContext> _predicate;
        private readonly RequestDelegate _requestHandler;

        public RequestDelegateEventHandler(
            Predicate<SlackEventContext> predicate,
            RequestDelegate requestHandler)
        {
            _predicate = predicate;
            _requestHandler = requestHandler;
        }

        public async ValueTask<SlackEventHandlerResult> HandleAsync(SlackEventContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!_predicate(context))
            {
                return SlackEventHandlerResult.NoResult();
            }

            await _requestHandler(context.HttpContext);
            return SlackEventHandlerResult.EndResponse();
        }
    }
}
