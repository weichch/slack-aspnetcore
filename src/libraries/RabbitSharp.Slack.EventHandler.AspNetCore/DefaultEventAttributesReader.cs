using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Implements <see cref="IEventAttributesReader"/> using <see cref="JsonSerializer"/>.
    /// This implementation is per-request service.
    /// </summary>
    class DefaultEventAttributesReader : IEventAttributesReader
    {
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly Dictionary<Type, object?> _objectCache;

        public DefaultEventAttributesReader(ISlackEventHandlerServicesFeature feature)
        {
            _serializerOptions = feature.SerializerOptions;
            _objectCache = new Dictionary<Type, object?>();
        }

        public async ValueTask<T> ReadAsync<T>(EventAttributesReaderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (_objectCache.TryGetValue(typeof(T), out var resultObj))
            {
                return (T) resultObj!;
            }

            var eventStream = context.Event;
            if (eventStream.CanSeek)
            {
                eventStream.Seek(0, SeekOrigin.Begin);
            }

            T value;
            try
            {
                value = await JsonSerializer.DeserializeAsync<T>(
                    eventStream, _serializerOptions, context.CancellationToken);
            }
            catch (JsonException)
            {
                value = default;
            }

            _objectCache.Add(typeof(T), value);
            return value!;
        }
    }
}
