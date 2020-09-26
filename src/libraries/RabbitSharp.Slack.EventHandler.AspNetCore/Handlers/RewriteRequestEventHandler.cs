using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RabbitSharp.Slack.Events.Handlers
{
    /// <summary>
    /// Implements <see cref="ISlackEventHandler"/> by rewriting current HTTP request to an alternative path.
    /// </summary>
    class RewriteRequestEventHandler : ISlackEventHandler
    {
        private readonly Predicate<SlackEventContext> _predicate;
        private readonly Func<SlackEventContext, ValueTask<PathString>> _pathBuilder;

        public RewriteRequestEventHandler(
            Predicate<SlackEventContext> predicate,
            Func<SlackEventContext, ValueTask<PathString>> pathBuilder)
        {
            _predicate = predicate;
            _pathBuilder = pathBuilder;
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

            var newPath = await _pathBuilder(context);
            return SlackEventHandlerResult.Rewrite(newPath);
        }
    }
}
