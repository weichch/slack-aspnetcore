using System.Text.Json;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Implements features provisioned by <see cref="SlackEventHandlerMiddleware"/>.
    /// </summary>
    class SlackEventHandlerFeature : ISlackEventHandlerServicesFeature, ISlackRequestVerificationFeature
    {
        public JsonSerializerOptions SerializerOptions { get; set; } = null!;
        public ISlackRequestValidator RequestValidator { get; set; } = null!;
        public IEventAttributesReader AttributesReader { get; set; } = null!;
        public SlackRequestValidationParameters? Parameters { get; set; }
    }
}
