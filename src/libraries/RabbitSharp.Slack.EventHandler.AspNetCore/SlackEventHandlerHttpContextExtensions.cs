using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Provides extensions to <see cref="HttpContext"/> for Slack event handlers.
    /// </summary>
    public static class SlackEventHandlerHttpContextExtensions
    {
        private const string SlackRequestVerificationResultKey = "__SlackRequestVerificationResult";

        /// <summary>
        /// Gets <see cref="ISlackEventHandlerServicesFeature"/> provisioned by <see cref="SlackEventHandlerMiddleware"/>.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        internal static ISlackEventHandlerFeature GetSlackEventHandlerFeature(
            this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var feature = httpContext.Features.Get<ISlackEventHandlerFeature>();
            if (feature == null)
            {
                throw new InvalidOperationException(
                    "No feature found. Have you added SlackEventHandlerMiddleware?");
            }

            return feature;
        }

        /// <summary>
        /// Gets <see cref="ISlackEventHandlerServicesFeature"/> provisioned by <see cref="SlackEventHandlerMiddleware"/>.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        internal static ISlackEventHandlerServicesFeature GetSlackEventHandlerServicesFeature(
            this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var feature = httpContext.Features.Get<ISlackEventHandlerServicesFeature>();
            if (feature == null)
            {
                throw new InvalidOperationException(
                    "No feature found. Have you added SlackEventHandlerMiddleware?");
            }

            return feature;
        }

        /// <summary>
        /// Gets the instance of Slack request validator for verifying requests from Slack.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        internal static ISlackRequestValidator GetSlackRequestValidator(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            // If there is feature, the validator should have been provisioned
            var validator = httpContext.Features.Get<ISlackEventHandlerServicesFeature>()?.RequestValidator;
            if (validator != null)
            {
                return validator;
            }

            // Otherwise, try get cached validator
            if (httpContext.Items.TryGetValue(typeof(ISlackRequestValidator), out var validatorObj))
            {
                return (ISlackRequestValidator) validatorObj;
            }

            // If no cached, create a new instance and cache it
            validator = ActivatorUtilities.GetServiceOrCreateInstance<SlackRequestValidator>(
                httpContext.RequestServices);
            httpContext.Items[typeof(ISlackRequestValidator)] = validator;

            return validator;
        }

        /// <summary>
        /// Verifies the request from Slack.
        /// </summary>
        public static async ValueTask<bool> VerifySlackRequestAsync(this HttpContext httpContext)
        {
            bool verificationResult;
            if (httpContext.Items.ContainsKey(SlackRequestVerificationResultKey))
            {
                // Previously verified
                verificationResult = (bool) httpContext.Items[SlackRequestVerificationResultKey];
            }
            else
            {
                while (true)
                {
                    var parameters = httpContext.GetSlackEventHandlerFeature().Options.RequestValidationParameters;
                    if (parameters == null)
                    {
                        // There is no way to validate the request, so trust it.
                        verificationResult = true;
                        break;
                    }

                    var validator = httpContext.GetSlackRequestValidator();
                    verificationResult = await validator.VerifyRequestAsync(httpContext, parameters);
                    break;
                }

                httpContext.Items[SlackRequestVerificationResultKey] = verificationResult;
            }

            if (!verificationResult && !httpContext.Response.HasStarted)
            {
                // The request isn't from Slack, end the response with an error status code
                httpContext.Response.Clear();
                httpContext.Response.StatusCode = StatusCodes.Status406NotAcceptable;
            }

            return verificationResult;
        }
    }
}
