using System;
using Microsoft.Extensions.Options;
using RabbitSharp.Slack.Events;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Provides extensions to <see cref="IApplicationBuilder"/> for adding Slack event handlers.
    /// </summary>
    public static class SlackEventHandlerBuilderExtensions
    {
        /// <summary>
        /// Adds a middleware which receives event notification requests from Slack, verify them,
        /// and re-execute them in alternative request pipeline. This middleware does not handle
        /// request if response has started.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="optionsBuilder">The options builder.</param>
        public static IApplicationBuilder UseSlackEventHandler(
            this IApplicationBuilder app,
            Action<SlackEventHandlerOptions> optionsBuilder)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (optionsBuilder == null)
            {
                throw new ArgumentNullException(nameof(optionsBuilder));
            }

            var options = new SlackEventHandlerOptions(app);
            optionsBuilder.Invoke(options);
            return app.UseMiddleware<SlackEventHandlerMiddleware>(Options.Create(options));
        }
    }
}
