using System.Text.Json.Serialization;

namespace RabbitSharp.Slack.Events.Models
{
    /// <summary>
    /// Represents content of a URL verification handshake request.
    /// Refer to https://api.slack.com/events-api#the-events-api__subscribing-to-event-types__events-api-request-urls__request-url-configuration--verification__url-verification-handshake.
    /// </summary>
    public class UrlVerification
    {
        /// <summary>
        /// Creates an instance of the event wrapper;
        /// </summary>
        public UrlVerification()
        {
            Token = string.Empty;
            Challenge = string.Empty;
            Type = string.Empty;
        }

        /// <summary>
        /// Gets or sets the <c>token</c> field.
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the <c>challenge</c> field.
        /// </summary>
        [JsonPropertyName("challenge")]
        public string Challenge { get; set; }

        /// <summary>
        /// Gets or sets the <c>type</c> field.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
