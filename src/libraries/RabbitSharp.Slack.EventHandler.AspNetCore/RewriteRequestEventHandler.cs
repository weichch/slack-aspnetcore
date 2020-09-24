using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RabbitSharp.Slack.Events.Models;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Implements <see cref="ISlackEventHandler"/> by rewriting current HTTP request to an alternative path.
    /// </summary>
    class RewriteRequestEventHandler : ISlackEventHandler
    {
        private readonly Predicate<EventWrapper> _predicate;
        private readonly Func<SlackEventContext, EventWrapper, PathString> _pathBuilder;

        public RewriteRequestEventHandler(
            Predicate<EventWrapper> predicate, 
            Func<SlackEventContext, EventWrapper, PathString> pathBuilder)
        {
            _predicate = predicate;
            _pathBuilder = pathBuilder;
        }

        public ValueTask<SlackEventHandlerResult> HandleAsync(SlackEventContext context)
        {
            if (context.EventAttributes is EventWrapper eventWrapper)
            {
                if (!_predicate(eventWrapper))
                {
                    return SlackEventHandlerResult.NoResult();
                }

                var newPath = _pathBuilder(context, eventWrapper);
                return SlackEventHandlerResult.Rewrite(newPath);
            }

            return SlackEventHandlerResult.NoResult();
        }
    }
}
