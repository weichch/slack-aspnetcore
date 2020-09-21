using System.Threading.Tasks;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Defines a type which handles event notification from Slack.
    /// </summary>
    public interface ISlackEventHandler
    {
        /// <summary>
        /// Handles Slack event notification in a verified request context.
        /// </summary>
        /// <param name="context">The event context.</param>
        ValueTask<SlackEventHandlerResult> HandleAsync(SlackEventContext context);
    }
}
