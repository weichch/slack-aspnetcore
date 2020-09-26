using System;
using System.Text.Json;
using RabbitSharp.Slack.Events.Models;
using RabbitSharp.Slack.Events.Tests.App;
using Xunit;

namespace RabbitSharp.Slack.Events.Tests.Tests
{
    public class SerializationTests
    {
        [Fact]
        public void ShouldDeserializeEventWrapperWithoutExtensions()
        {
            var data = AppTests.CreateEventBody();
            var buffer = JsonSerializer.SerializeToUtf8Bytes(data);
            var serializerOptions = new SlackEventHandlerOptions().GetOrCreateJsonSerializerOptions();
            serializerOptions.WriteIndented = true;

            JsonSerializer.Deserialize<EventWrapper>(buffer.AsSpan(), serializerOptions);
        }

        [Fact]
        public void ShouldDeserializeEventWrapperWithExtensions()
        {
            var data = AppTests.CreateEventBody();
            var buffer = JsonSerializer.SerializeToUtf8Bytes(data);
            var serializerOptions = new SlackEventHandlerOptions {DeserializeAdditionalProperties = true}
                .GetOrCreateJsonSerializerOptions();
            serializerOptions.WriteIndented = true;

            var result = JsonSerializer.Deserialize<EventWrapper>(buffer.AsSpan(), serializerOptions);

            Assert.True(result.Extensions.Count > 0);
        }

        [Fact]
        public void ShouldSerializeEventWrapperWithoutExtensions()
        {
            var data = new EventWrapper
            {
                AuthedUsers = {Guid.NewGuid().ToString()}
            };
            var serializerOptions = new SlackEventHandlerOptions().GetOrCreateJsonSerializerOptions();
            serializerOptions.WriteIndented = true;

            var result = JsonSerializer.Serialize(data, serializerOptions);

            JsonSerializer.Deserialize<EventWrapper>(result);
        }

        [Fact]
        public void ShouldSerializeEventWrapperWithExtensions()
        {
            var data = new EventWrapper
            {
                AuthedUsers = {Guid.NewGuid().ToString()},
                Extensions =
                {
                    {"enterprise_id", Guid.NewGuid().ToString()}
                }
            };
            var serializerOptions = new SlackEventHandlerOptions {DeserializeAdditionalProperties = true}
                .GetOrCreateJsonSerializerOptions();
            serializerOptions.WriteIndented = true;

            var result = JsonSerializer.Serialize(data, serializerOptions);

            JsonSerializer.Deserialize<EventWrapper>(result);
        }

        [Fact]
        public void ShouldSerializeDeserializedWithoutExtensions()
        {
            var data = AppTests.CreateEventBody();
            var buffer = JsonSerializer.SerializeToUtf8Bytes(data);
            var serializerOptions = new SlackEventHandlerOptions().GetOrCreateJsonSerializerOptions();
            serializerOptions.WriteIndented = true;

            var instance1 = JsonSerializer.Deserialize<EventWrapper>(buffer.AsSpan(), serializerOptions);
            var json1 = JsonSerializer.Serialize(instance1, serializerOptions);
            var instance2 = JsonSerializer.Deserialize<EventWrapper>(json1, serializerOptions);
            var json2 = JsonSerializer.Serialize(instance2, serializerOptions);

            Assert.Equal(json1, json2);
        }

        [Fact]
        public void ShouldSerializeDeserializedWithExtensions()
        {
            var data = AppTests.CreateEventBody();
            var buffer = JsonSerializer.SerializeToUtf8Bytes(data);
            var serializerOptions = new SlackEventHandlerOptions {DeserializeAdditionalProperties = true}
                .GetOrCreateJsonSerializerOptions();
            serializerOptions.WriteIndented = true;

            var instance1 = JsonSerializer.Deserialize<EventWrapper>(buffer.AsSpan(), serializerOptions);
            var json1 = JsonSerializer.Serialize(instance1, serializerOptions);
            var instance2 = JsonSerializer.Deserialize<EventWrapper>(json1, serializerOptions);
            var json2 = JsonSerializer.Serialize(instance2, serializerOptions);

            Assert.Equal(json1, json2);
        }
    }
}
