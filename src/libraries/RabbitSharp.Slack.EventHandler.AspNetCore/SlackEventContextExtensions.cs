using System;
using System.Threading.Tasks;
using RabbitSharp.Slack.Events.Models;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Provides extension methods to <see cref="SlackEventContext"/>.
    /// </summary>
    public static class SlackEventContextExtensions
    {
        /// <summary>
        /// Attempts to read <see cref="UrlVerification"/> from event context.
        /// </summary>
        /// <param name="context">The event context.</param>
        public static ValueTask<UrlVerification?> GetUrlVerificationAttributes(
            this SlackEventContext context)
        {
            return context.GetEventAttributes<UrlVerification?>();
        }

        /// <summary>
        /// Attempts to read <see cref="EventWrapper"/> from event context.
        /// </summary>
        /// <param name="context">The event context.</param>
        public static ValueTask<EventWrapper?> GetEventAttributes(
            this SlackEventContext context)
        {
            return context.GetEventAttributes<EventWrapper?>();
        }

        /// <summary>
        /// Attempts to read instance of <typeparamref name="T"/> from event context.
        /// </summary>
        /// <param name="context">The event context.</param>
        public static async ValueTask<T> GetEventAttributes<T>(
            this SlackEventContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            await context.FetchEventAttributesAsync();

            if (context.EventAttributes is T result)
            {
                return result;
            }

            return default!;
        }
    }
}
