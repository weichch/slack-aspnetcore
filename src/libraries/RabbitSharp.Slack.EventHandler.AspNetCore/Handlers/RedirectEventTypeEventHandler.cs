using System;
using System.Threading.Tasks;

namespace RabbitSharp.Slack.Events.Handlers
{
    /// <summary>
    /// Implements <see cref="ISlackEventHandler"/> by redirecting current HTTP request
    /// when event type is equal to specified value.
    /// </summary>
    class RedirectEventTypeEventHandler : ISlackEventHandler
    {
        private readonly string _eventType;
        private readonly Uri _location;

        public RedirectEventTypeEventHandler(string eventType, Uri location)
        {
            _eventType = eventType;
            _location = location;
        }

        public ValueTask<SlackEventHandlerResult> HandleAsync(SlackEventContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var result = string.Equals(context.EventType, _eventType, StringComparison.OrdinalIgnoreCase)
                ? SlackEventHandlerResult.Redirect(_location)
                : SlackEventHandlerResult.NoResult();

            return new ValueTask<SlackEventHandlerResult>(result);
        }
    }
}
