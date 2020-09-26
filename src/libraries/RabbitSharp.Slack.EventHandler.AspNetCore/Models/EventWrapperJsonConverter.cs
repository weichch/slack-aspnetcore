using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RabbitSharp.Slack.Events.Models
{
    /// <summary>
    /// Represents <see cref="JsonConverter"/> for <see cref="EventWrapper"/>.
    /// </summary>
    public class EventWrapperJsonConverter : JsonConverter<EventWrapper>
    {
        /// <summary>
        /// Returns <c>type</c> in JSON encoded format.
        /// </summary>
        public static readonly JsonEncodedText Token = JsonEncodedText.Encode("token");

        /// <summary>
        /// Returns <c>team_id</c> in JSON encoded format.
        /// </summary>
        public static readonly JsonEncodedText TeamId = JsonEncodedText.Encode("team_id");

        /// <summary>
        /// Returns <c>api_app_id</c> in JSON encoded format.
        /// </summary>
        public static readonly JsonEncodedText ApplicationId = JsonEncodedText.Encode("api_app_id");

        /// <summary>
        /// Returns <c>event</c> in JSON encoded format.
        /// </summary>
        public static readonly JsonEncodedText Event = JsonEncodedText.Encode("event");

        /// <summary>
        /// Returns <c>type</c> in JSON encoded format.
        /// </summary>
        public static readonly JsonEncodedText Type = JsonEncodedText.Encode("type");

        /// <summary>
        /// Returns <c>event_id</c> in JSON encoded format.
        /// </summary>
        public static readonly JsonEncodedText EventId = JsonEncodedText.Encode("event_id");

        /// <summary>
        /// Returns <c>event_time</c> in JSON encoded format.
        /// </summary>
        public static readonly JsonEncodedText EventTime = JsonEncodedText.Encode("event_time");

        /// <summary>
        /// Returns <c>authed_users</c> in JSON encoded format.
        /// </summary>
        public static readonly JsonEncodedText AuthedUsers = JsonEncodedText.Encode("authed_users");

        /// <summary>
        /// Gets or sets whether to read and write additional properties as extensions.
        /// </summary>
        public bool SupportExtensions { get; set; }

        /// <summary>
        /// Deserializes <see cref="EventWrapper"/> from its JSON representation.
        /// </summary>
        public override EventWrapper Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.ReadJsonBlock<EventWrapper>(options, null, ReadProperties);
        }

        private void ReadProperties(ref Utf8JsonReader reader, EventWrapper eventWrapper, JsonSerializerOptions serializerOptions)
        {
            if (reader.TryReadStringProperty(Token, out var strValue))
            {
                eventWrapper.Token = strValue;
            }
            else if (reader.TryReadStringProperty(TeamId, out strValue))
            {
                eventWrapper.TeamId = strValue;
            }
            else if (reader.TryReadStringProperty(ApplicationId, out strValue))
            {
                eventWrapper.ApplicationId = strValue;
            }
            else if (reader.TryReadEventProperties(Event, serializerOptions, eventWrapper))
            {
                // Do nothing
            }
            else if (reader.TryReadStringProperty(Type, out strValue))
            {
                eventWrapper.Type = strValue;
            }
            else if (reader.TryReadStringProperty(EventId, out strValue))
            {
                eventWrapper.EventId = strValue;
            }
            else if (reader.TryReadInt64Property(EventTime, out var longValue))
            {
                eventWrapper.EventTime = longValue;
            }
            else if (reader.TryReadStringArrayProperty(AuthedUsers, (List<string>) eventWrapper.AuthedUsers))
            {
                // Do nothing
            }
            else if (SupportExtensions)
            {
                reader.ReadExtension(eventWrapper.Extensions, serializerOptions);
            }
            else
            {
                reader.SkipProperty();
            }
        }

        /// <summary>
        /// Serializes <see cref="EventWrapper"/> to its JSON representation.
        /// </summary>
        public override void Write(Utf8JsonWriter writer, EventWrapper value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString(Token, value.Type);
            writer.WriteString(TeamId, value.TeamId);
            writer.WriteString(ApplicationId, value.ApplicationId);

            writer.WritePropertyName(Event);
            JsonSerializer.Serialize(writer, value.Event, value.Event?.GetType() ?? typeof(EventProperties), options);

            writer.WriteString(Type, value.Type);
            writer.WriteString(EventId, value.EventId);
            writer.WriteNumber(EventTime, value.EventTime);

            writer.WritePropertyName(AuthedUsers);
            writer.WriteStartArray();

            foreach (var stringValue in value.AuthedUsers)
            {
                writer.WriteStringValue(stringValue);
            }

            writer.WriteEndArray();

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
