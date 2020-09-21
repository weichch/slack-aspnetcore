using System.Text.Json.Serialization;

namespace RabbitSharp.Slack.Events.Models
{
    /// <summary>
    /// Represents the required properties on all event types defined as per JSON schema
    /// at https://api.slack.com/types/event.
    /// </summary>
    public class EventProperties
    {
        /// <summary>
        /// Represents the <c>type</c> property.
        /// </summary>
        [JsonPropertyName("type")]
        public string EventType { get; set; } = null!;

        /// <summary>
        /// Represents the <c>event_ts</c> property.
        /// </summary>
        [JsonPropertyName("event_ts")]
        public string Timestamp { get; set; } = null!;
    }
}
