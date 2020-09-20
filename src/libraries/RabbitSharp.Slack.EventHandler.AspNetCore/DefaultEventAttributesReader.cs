using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using static RabbitSharp.Slack.Events.SlackEventHandlerConstants;

namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Implements <see cref="IEventAttributesReader"/> using <see cref="JsonSerializer"/>.
    /// </summary>
    class DefaultEventAttributesReader : IEventAttributesReader
    {
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly Dictionary<Type, object> _objectCache;

        public DefaultEventAttributesReader(ISlackEventHandlerServicesFeature services)
        {
            _serializerOptions = services.SerializerOptions;
            _objectCache = new Dictionary<Type, object>();
        }

        public async ValueTask<T> ReadAsync<T>(EventAttributesReaderContext context)
        {
            if (!string.Equals(
                context.HttpContext.Request.ContentType,
                ContentTypeApplicationJson,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Cannot read request content.");
            }

            if (_objectCache.TryGetValue(typeof(T), out var resultObj))
            {
                return (T) resultObj;
            }

            var eventStream = context.Event;
            if (eventStream.CanSeek)
            {
                eventStream.Seek(0, SeekOrigin.Begin);
            }

            var value = await JsonSerializer.DeserializeAsync<T>(
                eventStream, _serializerOptions, context.CancellationToken);
            _objectCache.Add(typeof(T), value!);
            return value;
        }
    }
}
