using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RabbitSharp.Slack.Events.Models
{
    /// <summary>
    /// Represents <see cref="JsonConverter"/> for <see cref="EventProperties"/>.
    /// </summary>
    public class EventPropertiesJsonConverter : JsonConverter<EventProperties>
    {
        /// <summary>
        /// Returns <c>type</c> in JSON encoded format.
        /// </summary>
        public static readonly JsonEncodedText Type = JsonEncodedText.Encode("type");

        /// <summary>
        /// Returns <c>event_ts</c> in JSON encoded format.
        /// </summary>
        public static readonly JsonEncodedText EventTimestamp = JsonEncodedText.Encode("event_ts");

        /// <summary>
        /// Gets or sets whether to read and write additional properties as extensions.
        /// </summary>
        public bool SupportExtensions { get; set; }

        /// <summary>
        /// Deserializes <see cref="EventProperties"/> from its JSON representation.
        /// </summary>
        public override EventProperties Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Read(ref reader, typeToConvert, null, options);
        }

        /// <summary>
        /// Deserializes <see cref="EventProperties"/> from its JSON representation.
        /// </summary>
        public virtual EventProperties Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            EventProperties? existingValue,
            JsonSerializerOptions options)
        {
            return reader.ReadJsonBlock(options, existingValue, ReadProperties);
        }

        private void ReadProperties(
            ref Utf8JsonReader reader, 
            EventProperties eventProperties, 
            JsonSerializerOptions serializerOptions)
        {
            if (reader.TryReadStringProperty(Type, out var strValue))
            {
                eventProperties.Type = strValue;
            }
            else if (reader.TryReadStringProperty(EventTimestamp, out strValue))
            {
                eventProperties.EventTimestamp = strValue;
            }
            else if (SupportExtensions)
            {
                reader.ReadExtension(eventProperties.Extensions, serializerOptions);
            }
            else
            {
                reader.SkipProperty();
            }
        }

        /// <summary>
        /// Serializes <see cref="EventProperties"/> to its JSON representation.
        /// </summary>
        public override void Write(Utf8JsonWriter writer, EventProperties value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString(Type, value.Type);
            writer.WriteString(EventTimestamp, value.EventTimestamp);

            if (SupportExtensions)
            {
                foreach (var (propName, propValue) in value.Extensions)
                {
                    writer.WritePropertyName(propName);
                    JsonSerializer.Serialize(writer, propValue, propValue?.GetType() ?? typeof(object), options);
                }
            }

            writer.WriteEndObject();
        }
    }
}
