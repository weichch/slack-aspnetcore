using System;
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
        /// Creates an instance of the event wrapper;
        /// </summary>
        public EventWrapper()
        {
            Token = string.Empty;
            TeamId = string.Empty;
            EnterpriseId = string.Empty;
            ApplicationId = string.Empty;
            EventDispatchType = string.Empty;
            EventId = string.Empty;
            Timestamp = -1;
            AuthedUsers = Array.Empty<string>();
            Event = new EventProperties
            {
                EventType = string.Empty,
                Timestamp = string.Empty
            };
        }

        /// <summary>
        /// Represents the <c>token</c> property.
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; set; }

        /// <summary>
        /// Represents the <c>team_id</c> property.
        /// </summary>
        [JsonPropertyName("team_id")]
        public string TeamId { get; set; }

        /// <summary>
        /// Represents the <c>enterprise_id</c> property.
        /// </summary>
        [JsonPropertyName("enterprise_id")]
        public string EnterpriseId { get; set; }

        /// <summary>
        /// Represents the <c>api_app_id</c> property.
        /// </summary>
        [JsonPropertyName("api_app_id")]
        public string ApplicationId { get; set; }

        /// <summary>
        /// Represents the <c>event</c> property.
        /// </summary>
        [JsonPropertyName("event")]
        public EventProperties Event { get; set; }

        /// <summary>
        /// Represents the <c>type</c> property.
        /// </summary>
        [JsonPropertyName("type")]
        public string EventDispatchType { get; set; }

        /// <summary>
        /// Represents the <c>event_id</c> property.
        /// </summary>
        [JsonPropertyName("event_id")]
        public string EventId { get; set; }

        /// <summary>
        /// Represents the <c>event_time</c> property.
        /// </summary>
        [JsonPropertyName("event_time")]
        public long Timestamp { get; set; }

        /// <summary>
        /// Represents the <c>authed_users</c> property.
        /// </summary>
        [JsonPropertyName("authed_users")]
        public IReadOnlyList<string> AuthedUsers { get; set; }
    }
}
