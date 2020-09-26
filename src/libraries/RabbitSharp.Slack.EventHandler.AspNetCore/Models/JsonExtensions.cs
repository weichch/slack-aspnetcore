using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace RabbitSharp.Slack.Events.Models
{
    internal delegate void ReadJsonValue<in T>(
        ref Utf8JsonReader reader, T instance, JsonSerializerOptions serializerOptions)
        where T : class;

    /// <summary>
    /// Provides JSON extensions.
    /// </summary>
    static class JsonExtensions
    {
        /// <summary>
        /// Returns <c>type</c> in JSON encoded format.
        /// </summary>
        public static readonly JsonEncodedText Type = JsonEncodedText.Encode("type");

        /// <summary>
        /// Returns <c>event</c> in JSON encoded format.
        /// </summary>
        public static readonly JsonEncodedText Event = JsonEncodedText.Encode("event");

        public static T ReadJsonBlock<T>(
            this ref Utf8JsonReader reader,
            JsonSerializerOptions serializerOptions,
            T? existingValue,
            ReadJsonValue<T> valueSetter)
            where T : class, new()
        {
            var startToken = reader.TokenType;
            var endToken = startToken switch
            {
                JsonTokenType.StartObject => JsonTokenType.EndObject,
                JsonTokenType.StartArray => JsonTokenType.EndArray,
                _ => throw UnexpectedTokenException()
            };

            var value = existingValue ?? new T();
            while (true)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    if (!reader.Read())
                    {
                        break;
                    }
                }

                if (reader.TokenType == endToken)
                {
                    break;
                }

                valueSetter(ref reader, value, serializerOptions);
            }

            if (reader.TokenType != endToken)
            {
                throw UnexpectedTokenException();
            }

            return value!;

            static Exception UnexpectedTokenException()
            {
                return new JsonException("Unexpected JSON token.");
            }
        }

        public static bool TryReadStringProperty(
            this ref Utf8JsonReader reader,
            JsonEncodedText propertyName,
            [NotNullWhen(true)] out string? value)
        {
            if (!reader.ValueTextEquals(propertyName.EncodedUtf8Bytes))
            {
                value = default;
                return false;
            }

            reader.Read();
            value = reader.GetString()!;
            return true;
        }

        public static bool TryReadInt64Property(
            this ref Utf8JsonReader reader,
            JsonEncodedText propertyName,
            out long value)
        {
            if (!reader.ValueTextEquals(propertyName.EncodedUtf8Bytes))
            {
                value = default;
                return false;
            }

            reader.Read();
            value = reader.GetInt64();
            return true;
        }

        public static bool TryReadStringArrayProperty(
            this ref Utf8JsonReader reader,
            JsonEncodedText propertyName,
            List<string> list)
        {
            if (!reader.ValueTextEquals(propertyName.EncodedUtf8Bytes))
            {
                return false;
            }

            reader.Read();
            reader.ReadJsonBlock(null!, list, ReadStringList);

            return true;

            static void ReadStringList(
                ref Utf8JsonReader reader,
                List<string> list,
                JsonSerializerOptions _)
            {
                list.Add(reader.GetString()!);
            }
        }

        public static bool TryReadEventProperties(
            this ref Utf8JsonReader reader,
            JsonEncodedText propertyName,
            JsonSerializerOptions options,
            EventWrapper eventWrapper)
        {
            if (!reader.ValueTextEquals(propertyName.EncodedUtf8Bytes))
            {
                return false;
            }

            reader.Read();

            var converter = options.GetConverter(typeof(EventProperties));
            if (converter is EventPropertiesJsonConverter propertiesConverter)
            {
                propertiesConverter.Read(ref reader, typeof(EventProperties), eventWrapper.Event, options);
                return true;
            }

            eventWrapper.Event = JsonSerializer.Deserialize<EventProperties>(ref reader, options)!;
            return true;
        }

        public static void ReadExtension(
            this ref Utf8JsonReader reader, 
            IDictionary<string, object> extensions,
            JsonSerializerOptions serializerOptions)
        {
            var propertyName = reader.GetString()!;
            reader.Read();
            var value = JsonSerializer.Deserialize(ref reader, typeof(object), serializerOptions);
            if (value != null)
            {
                extensions[propertyName] = value;
            }
        }

        public static void SkipProperty(this ref Utf8JsonReader reader)
        {
            reader.Read();
            reader.Skip();
        }

        public static void ReadEventTypes(
            this JsonDocument jsonDoc,
            out string? eventDispatchType,
            out string? eventType)
        {
            if (jsonDoc.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException(
                    "Unable to determine event type. Root element is not an object.");
            }

            eventDispatchType = eventType = null;

            using var enumerator = jsonDoc.RootElement.EnumerateObject();
            while (enumerator.MoveNext())
            {
                var prop = enumerator.Current;
                if (TryReadType(prop, out var value))
                {
                    eventDispatchType = value;
                }
                else if(prop.NameEquals(Event.EncodedUtf8Bytes))
                {
                    using var eventDataEnumerator = prop.Value.EnumerateObject();
                    while (eventDataEnumerator.MoveNext())
                    {
                        if (TryReadType(eventDataEnumerator.Current, out value))
                        {
                            eventType = value;
                            break;
                        }
                    }
                }
            }

            static bool TryReadType(JsonProperty property, out string? value)
            {
                if (!property.NameEquals(Type.EncodedUtf8Bytes))
                {
                    value = default;
                    return false;
                }

                value = property.Value.GetString();
                return true;
            }
        }
    }
}
