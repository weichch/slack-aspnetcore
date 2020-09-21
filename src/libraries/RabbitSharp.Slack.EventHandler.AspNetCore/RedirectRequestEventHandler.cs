using System;
using System.Threading.Tasks;
using RabbitSharp.Slack.Events.Models;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Implements <see cref="ISlackEventHandler"/> by redirecting current HTTP request.
    /// </summary>
    class RedirectRequestEventHandler : ISlackEventHandler
    {
        private readonly Predicate<EventWrapper> _predicate;
        private readonly Func<EventWrapper, Uri> _locationBuilder;

        public RedirectRequestEventHandler(
            Predicate<EventWrapper> predicate, 
            Func<EventWrapper, Uri> locationBuilder)
        {
            _predicate = predicate;
            _locationBuilder = locationBuilder;
        }

        public async ValueTask<SlackEventHandlerResult> HandleAsync(SlackEventContext context)
        {
            var eventWrapper = await context.ReadEventAttributes<EventWrapper>();

            if (!_predicate(eventWrapper))
            {
                return SlackEventHandlerResult.NoResult();
            }

            var location = _locationBuilder(eventWrapper);
            return SlackEventHandlerResult.Redirect(location);
        }
    }
}
