using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using RabbitSharp.Slack.Events.Tests.App;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RabbitSharp.Slack.Events.Tests.Tests
{
    public class RewriteTests : AppTests
    {
        [Fact]
        public async Task ShouldHandleGenericRewrite()
        {
            await Host.StartAsync();

            var body = CreateEventBody("custom_event1");
            var request = CreateSlackEventRequest("slack/event-hub", body);
            var response = await TestClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(JsonSerializer.Serialize(body, body.GetType()), content);
        }

        [Fact]
        public async Task ShouldHandleEventTypeRewrite()
        {
            await Host.StartAsync();

            var body = CreateEventBody("custom_event2");
            var request = CreateSlackEventRequest("slack/event-hub", body);
            var response = await TestClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(JsonSerializer.Serialize(body, body.GetType()), content);
        }

        [Fact]
        public async Task ShouldNotHandleUnknownEventTypeRewrite()
        {
            await Host.StartAsync();

            var request = CreateSlackEventRequest("slack/event-hub", CreateEventBody());
            var response = await TestClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.MisdirectedRequest, response.StatusCode);
        }

        [Fact]
        public async Task ShouldNotHandleUrlVerification()
        {
            await Host.StartAsync();

            var request = CreateSlackEventRequest("slack/event-hub", CreateUrlVerificationBody());
            var response = await TestClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.MisdirectedRequest, response.StatusCode);
        }

        protected override void ConfigureSlackEventHandler(SlackEventHandlerOptions options)
        {
            options.AddRewrite(
                e => e.Event.Type == "custom_event1",
                (context, e) => context.LinkHelper.GetPathByAction(
                    context.HttpContext,
                    action: "HandleCustomEvent",
                    controller: "Event",
                    values: new {eventId = e.Event.Type}));

            options.AddEventTypeRewrite("custom_event2", "/events/custom_event2");
        }
    }
}
