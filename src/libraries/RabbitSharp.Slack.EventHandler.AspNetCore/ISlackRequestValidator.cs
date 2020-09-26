using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RabbitSharp.Slack.Http
{
    /// <summary>
    /// Provides mechanisms to verify requests from Slack.
    /// Refer to https://api.slack.com/authentication/verifying-requests-from-slack.
    /// </summary>
    public interface ISlackRequestValidator
    {
        /// <summary>
        /// Verifies the request by deprecated verification token configured in App Credentials
        /// in Slack.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="parameters">The request validation parameters.</param>
        Task<bool> VerifyRequestAsync(HttpContext httpContext, SlackRequestValidationParameters parameters);
    }
}
