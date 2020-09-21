using System;

namespace RabbitSharp.Slack.Events.Models
{
    /// <summary>
    /// Provides extension methods to <see cref="EventWrapper"/>.
    /// </summary>
    public static class EventWrapperExtensions
    {
        /// <summary>
        /// Returns whether the wrapped event is of specified type.
        /// </summary>
        /// <param name="eventWrapper">The event wrapper.</param>
        /// <param name="eventType">The event type.</param>
        public static bool IsOfType(this EventWrapper eventWrapper, string eventType)
        {
            return string.Equals(eventWrapper.Event.EventType, eventType, StringComparison.OrdinalIgnoreCase);
        }
    }
}
