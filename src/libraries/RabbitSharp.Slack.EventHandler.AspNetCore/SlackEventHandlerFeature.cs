using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitSharp.Slack.Http;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Implements features provisioned by <see cref="SlackEventHandlerMiddleware"/>.
    /// </summary>
    class SlackEventHandlerFeature : ISlackEventHandlerServicesFeature, ISlackRequestVerificationFeature
    {
        public LogLevel LogLevel { get; set; }
        public JsonSerializerOptions SerializerOptions { get; set; } = null!;
        public ISlackRequestValidator RequestValidator { get; set; } = null!;
        public IEventAttributesProvider EventAttributesProvider { get; set; } = null!;
        public SlackRequestValidationParameters? Parameters { get; set; }
    }
}
