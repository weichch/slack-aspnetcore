using System;
using System.Threading.Tasks;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Represents <see cref="ISlackEventHandler"/> which uses a delegate function to produce
    /// <see cref="SlackEventHandlerResult"/>.
    /// </summary>
    class DelegateBasedEventHandler : ISlackEventHandler
    {
        private readonly Func<SlackEventContext, ValueTask<SlackEventHandlerResult>> _resultFactory;

        public DelegateBasedEventHandler(
            Func<SlackEventContext, ValueTask<SlackEventHandlerResult>> resultFactory)
        {
            _resultFactory = resultFactory;
        }

        public ValueTask<SlackEventHandlerResult> HandleAsync(SlackEventContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return _resultFactory(context);
        }
    }
}
