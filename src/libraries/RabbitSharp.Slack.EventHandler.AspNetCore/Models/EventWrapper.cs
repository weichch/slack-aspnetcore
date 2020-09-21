using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RabbitSharp.Slack.Events.Models
{
    /// <summary>
    /// Represents Slack event wrapper object defined as per JSON schema at https://api.slack.com/types/event.
    /// </summary>
    public class EventWrapper
    {
        /// <summary>
        /// Represents the <c>token</c> property.
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; set; } = null!;

        /// <summary>
        /// Represents the <c>team_id</c> property.
        /// </summary>
        [JsonPropertyName("team_id")]
        public string TeamId { get; set; } = null!;

        /// <summary>
        /// Represents the <c>api_app_id</c> property.
        /// </summary>
        [JsonPropertyName("api_app_id")]
        public string ApplicationId { get; set; } = null!;

        /// <summary>
        /// Represents the <c>event</c> property.
        /// </summary>
        [JsonPropertyName("event")]
        public EventProperties Event { get; set; } = null!;

        /// <summary>
        /// Represents the <c>type</c> property.
        /// </summary>
        [JsonPropertyName("type")]
        public string EventDispatchType { get; set; } = null!;

        /// <summary>
        /// Represents the <c>event_id</c> property.
        /// </summary>
        [JsonPropertyName("event_id")]
        public string EventId { get; set; } = null!;

        /// <summary>
        /// Represents the <c>event_time</c> property.
        /// </summary>
        [JsonPropertyName("event_time")]
        public long Timestamp { get; set; }

        /// <summary>
        /// Represents the <c>authed_users</c> property.
        /// </summary>
        [JsonPropertyName("authed_users")]
        public IReadOnlyList<string> AuthedUsers { get; set; } = null!;
    }
}
