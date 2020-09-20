namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Provides constants.
    /// </summary>
    static class SlackEventHandlerConstants
    {
        public const string CurrentVersionNumber = "v0";

        public const string HttpHeaderSlackTimestamp = "X-Slack-Request-Timestamp";
        public const string HttpHeaderSlackSignature = "X-Slack-Signature";

        public const string ContentTypeApplicationJson = "application/json";
        public const string ContentTypePlainText = "text/plain";

        public const string EventTypeUrlVerification = "url_verification";
    }
}
