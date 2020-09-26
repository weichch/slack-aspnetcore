using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RabbitSharp.Slack.Events.Handlers
{
    /// <summary>
    /// Implements <see cref="ISlackEventHandler"/> by rewriting current HTTP request to an alternative path
    /// when event type is equal to specified value.
    /// </summary>
    class RewriteEventTypeEventHandler : ISlackEventHandler
    {
        private readonly string _eventType;
        private readonly Func<SlackEventContext, PathString> _pathBuilder;

        public RewriteEventTypeEventHandler(
            string eventType,
            Func<SlackEventContext, PathString> pathBuilder)
        {
            _eventType = eventType;
            _pathBuilder = pathBuilder;
        }

        public ValueTask<SlackEventHandlerResult> HandleAsync(SlackEventContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            SlackEventHandlerResult result;

            if (string.Equals(context.EventType, _eventType, StringComparison.OrdinalIgnoreCase))
            {
                var newPath = _pathBuilder(context);
                result = SlackEventHandlerResult.Rewrite(newPath);
            }
            else
            {
                result = SlackEventHandlerResult.NoResult();
            }

            return new ValueTask<SlackEventHandlerResult>(result);
        }
    }
}
