using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;

namespace RabbitSharp.Slack.Events
{
    // Forked from https://github.com/weichch/ExceptionMapper/blob/master/src/libraries/RabbitSharp.ExceptionMapper.AspNetCore/RoutePatternFormatter.cs

    /// <summary>
    /// Provides mechanism to format route pattern with route values.
    /// </summary>
    public class RoutePatternFormatter
    {
        private static readonly RouteValueDictionary EmptyRouteValues = new RouteValueDictionary();
        private readonly TemplateBinderFactory _templateBinderFactory;

        /// <summary>
        /// Creates an instance of the formatter.
        /// </summary>
        /// <param name="templateBinderFactory">The template binder factory.</param>
        public RoutePatternFormatter(TemplateBinderFactory templateBinderFactory)
        {
            _templateBinderFactory = templateBinderFactory;
        }

        /// <summary>
        /// Formats route pattern with ambient and specified route values.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="pattern">The route pattern.</param>
        /// <param name="routeValues">The route values.</param>
        public string? Format(HttpContext httpContext, RoutePattern pattern, object? routeValues)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            // Refer to
            // https://github.com/dotnet/aspnetcore/blob/master/src/Http/Routing/src/DefaultLinkGenerator.cs#L291
            var binder = _templateBinderFactory.Create(pattern);

            var valuesResult = binder.GetValues(EmptyRouteValues, routeValues == null
                ? EmptyRouteValues
                : new RouteValueDictionary(routeValues));

            if (valuesResult == null)
            {
                return null;
            }

            if (!binder.TryProcessConstraints(httpContext, valuesResult.CombinedValues, out var _, out var _))
            {
                return null;
            }

            return binder.BindValues(valuesResult.AcceptedValues);
        }
    }
}