using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
using static RabbitSharp.Slack.Events.SlackEventHandlerConstants;

namespace RabbitSharp.Slack.Http
{
    /// <summary>
    /// Provides parameters for verifying Slack requests.
    /// </summary>
    public class SlackRequestValidationParameters
    {
        /// <summary>
        /// Creates an instance of the parameters.
        /// </summary>
        public SlackRequestValidationParameters()
        {
            VersionNumber = CurrentVersionNumber;
            DriftTime = TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// Gets the version number. Value is always <c>v0</c> for now.
        /// </summary>
        public string VersionNumber { get; }

        /// <summary>
        /// Gets or sets the Signing Secret configured in App Credentials for your Slack app.
        /// </summary>
        public string? SigningSecret { get; set; }

        /// <summary>
        /// Gets or sets signing secret provider. You can use this property instead of
        /// <see cref="SigningSecret"/> to avoid storing token in this object.
        /// </summary>
        public Func<HttpContext, string>? SigningSecretProvider { get; set; }

        /// <summary>
        /// Gets or sets the drift time to use when determining whether or not a request occurred recently.
        /// When evaluating this property, the absolute value is always used. If this property is equal to
        /// <see cref="Timeout.InfiniteTimeSpan"/>, timestamp will be not checked. Default value is 5 minutes.
        /// </summary>
        public TimeSpan DriftTime { get; set; }
    }
}
