﻿using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitSharp.Slack.Events.Models;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Implements <see cref="IEventAttributesProvider"/> using <see cref="JsonSerializer"/>.
    /// This implementation is per-request service.
    /// </summary>
    class EventAttributesProvider : IEventAttributesProvider
    {
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly LogLevel _logLevel;
        private readonly ILogger _logger;

        public EventAttributesProvider(
            ISlackEventHandlerServicesFeature feature,
            ILoggerFactory loggerFactory)
        {
            _serializerOptions = feature.SerializerOptions;
            _logLevel = feature.LogLevel;
            _logger = loggerFactory.CreateLogger<EventAttributesProvider>();
        }

        public async ValueTask<object?> GetEventAttributes(EventAttributesProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            object? value;
            try
            {
                var eventAttributesType = context.EventContext switch
                {
                    var ctx when ctx.EventTypeEquals(
                        SlackEventHandlerConstants.EventTypeUrlVerification) => typeof(UrlVerification),
                    var ctx when string.IsNullOrWhiteSpace(ctx.EventType) => null,
                    _ => typeof(EventWrapper)
                };

                if (eventAttributesType == null)
                {
                    return null;
                }

                var eventStream = context.Event;
                if (eventStream.CanSeek)
                {
                    eventStream.Seek(0, SeekOrigin.Begin);
                }

                value = await JsonSerializer.DeserializeAsync(
                    eventStream, eventAttributesType, _serializerOptions, context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Log(_logLevel, ex, "Unable to read event attributes. Unexpected error occurred.");
                value = null;
            }

            return value;
        }
    }
}
