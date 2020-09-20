using System;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Provides extensions to build <see cref="SlackEventHandlerOptions"/>.
    /// </summary>
    public static class SlackEventHandlerOptionsExtensions
    {
        /// <summary>
        /// Adds event handler to handle URL verification event.
        /// </summary>
        public static SlackEventHandlerOptions AddUrlVerification(
            this SlackEventHandlerOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.EventsHandlers.Add(new UrlVerificationEventHandler());
            return options;
        }
    }
}
