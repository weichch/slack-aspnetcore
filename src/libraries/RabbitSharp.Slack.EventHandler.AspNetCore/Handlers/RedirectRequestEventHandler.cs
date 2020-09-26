﻿using System;
using System.Threading.Tasks;

namespace RabbitSharp.Slack.Events.Handlers
{
    /// <summary>
    /// Implements <see cref="ISlackEventHandler"/> by redirecting current HTTP request.
    /// </summary>
    class RedirectRequestEventHandler : ISlackEventHandler
    {
        private readonly Predicate<SlackEventContext> _predicate;
        private readonly Func<SlackEventContext, ValueTask<Uri>> _locationBuilder;

        public RedirectRequestEventHandler(
            Predicate<SlackEventContext> predicate, 
            Func<SlackEventContext, ValueTask<Uri>> locationBuilder)
        {
            _predicate = predicate;
            _locationBuilder = locationBuilder;
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

            var location = await _locationBuilder(context);
            return SlackEventHandlerResult.Redirect(location);
        }
    }
}
