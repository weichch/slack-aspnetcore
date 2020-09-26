using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RabbitSharp.Slack.Events.Handlers;
using RabbitSharp.Slack.Events.Models;

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
            Predicate<EventWrapper> predicate,
            Uri location)
        {
            SlackEventHandlerResult.EnsureRedirectLocation(location);
            return options.AddRedirect(predicate, _ => location);
        }

        /// <summary>
        /// Adds event handler which redirects request to a new location when event meets criteria.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="predicate">The predicate function.</param>
        /// <param name="locationBuilder">The new location builder.</param>
        public static SlackEventHandlerOptions AddRedirect(
            this SlackEventHandlerOptions options,
            Predicate<EventWrapper> predicate,
            Func<EventWrapper, Uri> locationBuilder)
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
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (eventType == null)
            {
                throw new ArgumentNullException(nameof(eventType));
            }

            SlackEventHandlerResult.EnsureRedirectLocation(location);
            options.EventsHandlers.Add(new RedirectEventTypeEventHandler(eventType, location));
            return options;
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
            Predicate<EventWrapper> predicate,
            PathString newPath)
        {
            return options.AddRewrite(predicate, (context, evt) => newPath);
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
            Predicate<EventWrapper> predicate,
            Func<SlackEventContext, EventWrapper, PathString> pathBuilder)
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
            return options.AddEventTypeRewrite(eventType, _ => newPath);
        }

        /// <summary>
        /// Adds event handler which rewrites request to a new path within the base path of the application
        /// when event type is equal to specified event type.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="eventType">The target event type.</param>
        /// <param name="pathBuilder">The path builder.</param>
        public static SlackEventHandlerOptions AddEventTypeRewrite(
            this SlackEventHandlerOptions options,
            string eventType,
            Func<SlackEventContext, PathString> pathBuilder)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (eventType == null)
            {
                throw new ArgumentNullException(nameof(eventType));
            }

            if (pathBuilder == null)
            {
                throw new ArgumentNullException(nameof(pathBuilder));
            }

            options.EventsHandlers.Add(new RewriteEventTypeEventHandler(eventType, pathBuilder));
            return options;
        }
    }
}
