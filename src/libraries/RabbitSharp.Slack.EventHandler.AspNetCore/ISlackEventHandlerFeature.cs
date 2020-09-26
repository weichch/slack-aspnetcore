using RabbitSharp.Slack.Http;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Represents feature for verifying requests from Slack. This feature is provisioned
    /// by <see cref="SlackEventHandlerMiddleware"/>.
    /// </summary>
    public interface ISlackRequestVerificationFeature
    {
        /// <summary>
        /// Gets or sets the request validation parameters.
        /// </summary>
        SlackRequestValidationParameters? Parameters { get; set; }
    }
}
