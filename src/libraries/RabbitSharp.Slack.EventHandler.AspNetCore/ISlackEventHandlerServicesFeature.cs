using System.Text.Json;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Provides services for build-in Slack event handler types.
    /// </summary>
    public interface ISlackEventHandlerServicesFeature
    {
        /// <summary>
        /// Gets the JSON serializer options.
        /// </summary>
        JsonSerializerOptions SerializerOptions { get; }

        /// <summary>
        /// Gets the request validator for verifying requests from Slack.
        /// </summary>
        ISlackRequestValidator RequestValidator { get; }

        /// <summary>
        /// Gets the event attributes reader.
        /// </summary>
        IEventAttributesReader AttributesReader { get; }
    }
}
