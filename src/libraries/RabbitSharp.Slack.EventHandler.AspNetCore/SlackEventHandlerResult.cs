using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Represents result of an <see cref="ISlackEventHandler"/>.
    /// </summary>
    public readonly struct SlackEventHandlerResult
    {
        private readonly List<Action<HttpContext>>? _resultActions;

        private SlackEventHandlerResult(List<Action<HttpContext>>? actions, bool redirect, bool rewrite)
        {
            IsHandled = true;
            _resultActions = actions;
            IsRedirect = redirect;
            IsRewrite = rewrite;
        }

        /// <summary>
        /// Indicates whether the result is handled.
        /// </summary>
        public bool IsHandled { get; }

        /// <summary>
        /// Indicates whether the result is a redirect result.
        /// </summary>
        public bool IsRedirect { get; }

        /// <summary>
        /// Indicates whether the result is a rewrite result.
        /// </summary>
        public bool IsRewrite { get; }

        /// <summary>
        /// Indicates whether the current result can accept callback registration.
        /// </summary>
        public bool CanRegisterCallback => _resultActions != null;

        /// <summary>
        /// Registers a callback function to run when the result is being applied to an HTTP context.
        /// </summary>
        /// <param name="callback">The callback function.</param>
        public void RegisterResultCallback(Action<HttpContext> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (!CanRegisterCallback)
            {
                throw new InvalidOperationException("Cannot register callback to current result.");
            }

            _resultActions!.Add(callback);
        }

        /// <summary>
        /// Applies the result to HTTP context.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        public void ApplyTo(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (_resultActions == null || _resultActions.Count == 0)
            {
                return;
            }

            foreach (var resultAction in _resultActions)
            {
                resultAction(httpContext);
            }
        }

        /// <summary>
        /// Indicates there is no event handler result.
        /// </summary>
        public static SlackEventHandlerResult NoResult()
        {
            return default;
        }

        /// <summary>
        /// Indicates the request should be redirected to the specified location.
        /// </summary>
        /// <param name="location">The new location.</param>
        public static SlackEventHandlerResult Redirect(Uri location)
        {
            EnsureRedirectLocation(location);

            var result = new SlackEventHandlerResult(
                new List<Action<HttpContext>>(), true, false);
            result.RegisterResultCallback(httpContext =>
            {
                httpContext.Response.Headers[HeaderNames.Location] = location.AbsoluteUri;
            });
            return result;
        }

        /// <summary>
        /// Indicates the request should be rewritten to another path within the base path of the application.
        /// </summary>
        /// <param name="path">The new path.</param>
        public static SlackEventHandlerResult Rewrite(PathString path)
        {
            var result = new SlackEventHandlerResult(
                new List<Action<HttpContext>>(), false, true);
            result.RegisterResultCallback(httpContext =>
            {
                var request = httpContext.Request;
                if (request.Body.CanSeek)
                {
                    request.Body.Seek(0, SeekOrigin.Begin);
                }
                request.Path = path;
            });
            return result;
        }

        /// <summary>
        /// Indicates a response has been provided and the request should be ended.
        /// </summary>
        public static SlackEventHandlerResult EndResponse()
        {
            return new SlackEventHandlerResult(null, false, false);
        }

        internal static void EnsureRedirectLocation(Uri location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (!location.IsAbsoluteUri)
            {
                throw new ArgumentException("Location must be an absolute URI.", nameof(location));
            }
        }

        /// <summary>
        /// Converts <see cref="SlackEventHandlerResult"/> to <see cref="ValueTask{T}"/>.
        /// </summary>
        public static implicit operator ValueTask<SlackEventHandlerResult>(SlackEventHandlerResult result)
        {
            return new ValueTask<SlackEventHandlerResult>(result);
        }
    }
}
