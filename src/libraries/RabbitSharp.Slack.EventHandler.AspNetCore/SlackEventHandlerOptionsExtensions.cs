using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using RabbitSharp.Slack.Events.Handlers;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Provides extensions to build <see cref="SlackEventHandlerOptions"/>.
    /// </summary>
    public static class SlackEventHandlerOptionsExtensions
    {
        /// <summary>
        /// Adds event handler to handle URL verification event.
        /// </summary>
        public static SlackEventHandlerOptions AddUrlVerification(
            this SlackEventHandlerOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.EventsHandlers.Add(new UrlVerificationEventHandler());
            return options;
        }

        /// <summary>
        /// Adds event handler which produces <see cref="SlackEventHandlerResult"/> using
        /// a delegate function.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="resultFactory">The delegate function which produces <see cref="SlackEventHandlerResult"/>.</param>
        public static SlackEventHandlerOptions AddDelegateHandler(
            this SlackEventHandlerOptions options,
            Func<SlackEventContext, ValueTask<SlackEventHandlerResult>> resultFactory)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (resultFactory == null)
            {
                throw new ArgumentNullException(nameof(resultFactory));
            }

            options.EventsHandlers.Add(new DelegateBasedEventHandler(resultFactory));
            return options;
        }

        /// <summary>
        /// Adds event handler which redirects request to a new location when event meets criteria.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="predicate">The predicate function.</param>
        /// <param name="location">The new location.</param>
        public static SlackEventHandlerOptions AddRedirect(
            this SlackEventHandlerOptions options,
            Predicate<SlackEventContext> predicate,
            Uri location)
        {
            SlackEventHandlerResult.EnsureRedirectLocation(location);
            return options.AddRedirect(predicate, _ => new ValueTask<Uri>(location));
        }

        /// <summary>
        /// Adds event handler which redirects request to a new location when event meets criteria.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="predicate">The predicate function.</param>
        /// <param name="locationBuilder">The new location builder.</param>
        public static SlackEventHandlerOptions AddRedirect(
            this SlackEventHandlerOptions options,
            Predicate<SlackEventContext> predicate,
            Func<SlackEventContext, Uri> locationBuilder)
        {
            if (locationBuilder == null)
            {
                throw new ArgumentNullException(nameof(locationBuilder));
            }

            return options.AddRedirect(predicate, context =>
                new ValueTask<Uri>(locationBuilder(context)));
        }

        /// <summary>
        /// Adds event handler which redirects request to a new location when event meets criteria.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="predicate">The predicate function.</param>
        /// <param name="locationBuilder">The new location builder.</param>
        public static SlackEventHandlerOptions AddRedirect(
            this SlackEventHandlerOptions options,
            Predicate<SlackEventContext> predicate,
            Func<SlackEventContext, ValueTask<Uri>> locationBuilder)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (locationBuilder == null)
            {
                throw new ArgumentNullException(nameof(locationBuilder));
            }

            options.EventsHandlers.Add(new RedirectRequestEventHandler(predicate, locationBuilder));
            return options;
        }

        /// <summary>
        /// Adds event handler which redirects request to a new location when event type is equal to
        /// specified event type.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="eventType">The target event type.</param>
        /// <param name="location">The new location.</param>
        public static SlackEventHandlerOptions AddEventTypeRedirect(
            this SlackEventHandlerOptions options,
            string eventType,
            Uri location)
        {
            return options.AddRedirect(context => context.EventTypeEquals(eventType), location);
        }

        /// <summary>
        /// Adds event handler which rewrites request to a new path within the base path of the application
        /// when event meets criteria.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="predicate">The predicate function.</param>
        /// <param name="newPath">The new path.</param>
        public static SlackEventHandlerOptions AddRewrite(
            this SlackEventHandlerOptions options,
            Predicate<SlackEventContext> predicate,
            PathString newPath)
        {
            return options.AddRewrite(predicate, _ => new ValueTask<PathString>(newPath));
        }

        /// <summary>
        /// Adds event handler which rewrites request to a new path within the base path of the application
        /// when event meets criteria.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="predicate">The predicate function.</param>
        /// <param name="pathBuilder">The path builder.</param>
        public static SlackEventHandlerOptions AddRewrite(
            this SlackEventHandlerOptions options,
            Predicate<SlackEventContext> predicate,
            Func<SlackEventContext, PathString> pathBuilder)
        {
            if (pathBuilder == null)
            {
                throw new ArgumentNullException(nameof(pathBuilder));
            }

            return options.AddRewrite(predicate, context =>
                new ValueTask<PathString>(pathBuilder(context)));
        }

        /// <summary>
        /// Adds event handler which rewrites request to a new path within the base path of the application
        /// when event meets criteria.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="predicate">The predicate function.</param>
        /// <param name="pathBuilder">The path builder.</param>
        public static SlackEventHandlerOptions AddRewrite(
            this SlackEventHandlerOptions options,
            Predicate<SlackEventContext> predicate,
            Func<SlackEventContext, ValueTask<PathString>> pathBuilder)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (pathBuilder == null)
            {
                throw new ArgumentNullException(nameof(pathBuilder));
            }

            options.EventsHandlers.Add(new RewriteRequestEventHandler(predicate, pathBuilder));
            return options;
        }

        /// <summary>
        /// Adds event handler which rewrites request to a new path within the base path of the application
        /// when event type is equal to specified event type.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="eventType">The target event type.</param>
        /// <param name="newPath">The new path.</param>
        public static SlackEventHandlerOptions AddEventTypeRewrite(
            this SlackEventHandlerOptions options,
            string eventType,
            PathString newPath)
        {
            return options.AddRewrite(
                context => context.EventTypeEquals(eventType),
                _ => new ValueTask<PathString>(newPath));
        }

        /// <summary>
        /// Adds event handler which handles event in an HTTP request handler.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="predicate">The predicate function.</param>
        /// <param name="requestHandler">The request handler.</param>
        public static SlackEventHandlerOptions AddRequestHandler(
            this SlackEventHandlerOptions options,
            Predicate<SlackEventContext> predicate,
            RequestDelegate requestHandler)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            options.EventsHandlers.Add(new RequestDelegateEventHandler(predicate, requestHandler));
            return options;
        }

        /// <summary>
        /// Adds event handler which handles event in an HTTP request handler.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="predicate">The predicate function.</param>
        /// <param name="requestHandlerBuilder">The request handler builder.</param>
        public static SlackEventHandlerOptions AddRequestHandler(
            this SlackEventHandlerOptions options,
            Predicate<SlackEventContext> predicate,
            Action<IApplicationBuilder> requestHandlerBuilder)
        {
            if (requestHandlerBuilder == null)
            {
                throw new ArgumentNullException(nameof(requestHandlerBuilder));
            }

            var appBuilder = options.ApplicationBuilder.New();
            requestHandlerBuilder(appBuilder);
            var requestHandler = appBuilder.Build();

            return options.AddRequestHandler(predicate, requestHandler);
        }

        /// <summary>
        /// Adds event handler which handles event in an HTTP request handler.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="eventType">The target event type.</param>
        /// <param name="requestHandler">The request handler.</param>
        public static SlackEventHandlerOptions AddEventTypeRequestHandler(
            this SlackEventHandlerOptions options,
            string eventType,
            RequestDelegate requestHandler)
        {
            return options.AddRequestHandler(
                context => context.EventTypeEquals(eventType),
                requestHandler);
        }

        /// <summary>
        /// Adds event handler which handles event in an HTTP request handler.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="eventType">The target event type.</param>
        /// <param name="requestHandlerBuilder">The request handler builder.</param>
        public static SlackEventHandlerOptions AddEventTypeRequestHandler(
            this SlackEventHandlerOptions options,
            string eventType,
            Action<IApplicationBuilder> requestHandlerBuilder)
        {
            return options.AddRequestHandler(
                context => context.EventTypeEquals(eventType),
                requestHandlerBuilder);
        }
    }
}
