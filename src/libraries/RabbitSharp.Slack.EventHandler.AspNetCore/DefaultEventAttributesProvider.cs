using System;
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
    class DefaultEventAttributesProvider : IEventAttributesProvider
    {
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly LogLevel _logLevel;
        private readonly ILogger _logger;

        public DefaultEventAttributesProvider(
            ISlackEventHandlerServicesFeature feature,
            ILoggerFactory loggerFactory)
        {
            _serializerOptions = feature.SerializerOptions;
            _logLevel = feature.LogLevel;
            _logger = loggerFactory.CreateLogger<DefaultEventAttributesProvider>();
        }

        public async ValueTask<object?> GetEventAttributes(EventAttributesProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var eventStream = context.Event;
            if (!eventStream.CanSeek)
            {
                // Can't read attributes when stream is not seekable.
                _logger.Log(_logLevel, "Unable to read event attributes. Event stream does not support seeking.");
                return null;
            }

            object? value;
            try
            {
                var eventType = await GetEventType(context);
                if (string.IsNullOrWhiteSpace(eventType))
                {
                    return null;
                }

                var eventAttributesType = eventType switch
                {
                    SlackEventHandlerConstants.EventTypeUrlVerification => typeof(UrlVerification),
                    _ => typeof(EventWrapper)
                };

                eventStream.Seek(0, SeekOrigin.Begin);
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

        private async ValueTask<string?> GetEventType(EventAttributesProviderContext context)
        {
            var eventStream = context.Event;
            eventStream.Seek(0, SeekOrigin.Begin);

            var jsonDoc = await JsonDocument.ParseAsync(
                eventStream,
                cancellationToken: context.CancellationToken);

            if (jsonDoc.RootElement.ValueKind != JsonValueKind.Object)
            {
                _logger.Log(_logLevel, "Unable to determine event type. Root element is not an object.");
                return null;
            }

            using var enumerator = jsonDoc.RootElement.EnumerateObject();
            while (enumerator.MoveNext())
            {
                var currentProp = enumerator.Current;
                if (currentProp.NameEquals("type")
                    && currentProp.Value.ValueKind == JsonValueKind.String)
                {
                    return currentProp.Value.GetString();
                }
            }

            _logger.Log(_logLevel, "Unable to determine event type. No 'type' property found.");
            return null;
        }
    }
}
