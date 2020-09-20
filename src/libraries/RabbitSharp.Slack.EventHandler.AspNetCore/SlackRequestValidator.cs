using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using static RabbitSharp.Slack.Events.SlackEventHandlerConstants;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Implements <see cref="ISlackRequestValidator"/>.
    /// </summary>
    public class SlackRequestValidator : ISlackRequestValidator
    {
        /// <summary>
        /// Implements verification by signing key as per
        /// https://api.slack.com/authentication/verifying-requests-from-slack#verifying-requests-from-slack-using-signing-secrets__a-recipe-for-security__how-to-make-a-request-signature-in-4-easy-steps-an-overview.
        /// </summary>
        public async Task<bool> VerifyRequestAsync(HttpContext httpContext, SlackRequestValidationParameters parameters)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var request = httpContext.Request;
            // Fail early if we can't seek to the beginning of the request body.
            if (!request.Body.CanSeek)
            {
                throw new InvalidOperationException(
                    "HTTP request body cannot be read more than once. Try turning request buffering on.");
            }

            var signingSecret = parameters.SigningSecret
                                ?? parameters.SigningSecretProvider?.Invoke(httpContext);
            if (string.IsNullOrWhiteSpace(signingSecret))
            {
                // There is no signing secret so we will fail the validation
                return false;
            }

            // If there is no signature, fail earlier.
            string? signature;
            if (!request.Headers.TryGetValue(HttpHeaderSlackSignature, out var values)
                || (signature = values.FirstOrDefault()) is null)
            {
                return false;
            }

            // No timestamp, fail earlier.
            if (!request.Headers.TryGetValue(HttpHeaderSlackTimestamp, out values)
                || !long.TryParse(values.FirstOrDefault(), out var timestamp))
            {
                return false;
            }

            if (parameters.DriftTime != Timeout.InfiniteTimeSpan)
            {
                // Validate timestamp must be within now +/- drift time
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var clockSkew = Convert.ToInt64(parameters.DriftTime.Duration().TotalSeconds);
                if (timestamp < now - clockSkew || timestamp > now + clockSkew)
                {
                    return false;
                }
            }

            // Validate body hash
            request.Body.Seek(0, SeekOrigin.Begin);
            using var bodyReader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await bodyReader.ReadToEndAsync();
            var bodyHash = CalculateRequestHash(signingSecret, parameters.VersionNumber, timestamp, body);

            return string.Equals(signature, bodyHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string CalculateRequestHash(
            string signingSecret,
            string version,
            long timestamp,
            string body)
        {
            // Refer to https://api.slack.com/authentication/verifying-requests-from-slack#verifying-requests-from-slack-using-signing-secrets__a-recipe-for-security__step-by-step-walk-through-for-validating-a-request

            var baseString = $"{version}:{timestamp:D}:{body}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(signingSecret));
            var hashed = hmac.ComputeHash(Encoding.UTF8.GetBytes(baseString));
            var hashedHex = BitConverter.ToString(hashed).Replace("-", string.Empty);
            return $"{version}={hashedHex}";
        }
    }
}
