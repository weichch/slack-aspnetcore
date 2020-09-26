using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RabbitSharp.Slack.Events.Models
{
    /// <summary>
    /// Represents the required properties on all event types defined as per
    /// JSON schema at https://api.slack.com/types/event.
    /// </summary>
    public class EventProperties
    {
        /// <summary>
        /// Creates an instance of the basic event properties.
        /// </summary>
        public EventProperties()
        {
            Type = string.Empty;
            EventTimestamp = string.Empty;
            Extensions = new Dictionary<string, object>(StringComparer.Ordinal);
        }

        /// <summary>
        /// Represents the <c>type</c> property.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Represents the <c>event_ts</c> property.
        /// </summary>
        [JsonPropertyName("event_ts")]
        public string EventTimestamp { get; set; }

        /// <summary>
        /// Gets additional properties.
        /// </summary>
        public IDictionary<string,object> Extensions { get; }
    }
}
