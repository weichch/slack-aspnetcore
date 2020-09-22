using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RabbitSharp.Slack.Events.Tests.App;
using Xunit;

namespace RabbitSharp.Slack.Events.Tests.Tests
{
    public class RedirectTests : AppTests
    {
        [Fact]
        public async Task ShouldHandleGenericRedirect()
        {
            await Host.StartAsync();

            var request = CreateSlackEventRequest("slack/event-hub", CreateEventBody("custom_event1"));
            var response = await TestClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
            Assert.Equal(new Uri("http://starwars.com/event/custom_event1"), response.Headers.Location);
        }

        [Fact]
        public async Task ShouldHandleEventTypeRedirect()
        {
            await Host.StartAsync();

            var request = CreateSlackEventRequest("slack/event-hub", CreateEventBody("custom_event2"));
            var response = await TestClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
            Assert.Equal(new Uri("http://starwars.com/event/custom_event2"), response.Headers.Location);
        }

        [Fact]
        public async Task ShouldNotHandleUnknownEventTypeRedirect()
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
            options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;

            options.AddRedirect(
                e => e.Event.EventType == "custom_event1",
                e => new Uri($"http://starwars.com/event/{e.Event.EventType}"));

            options.AddEventTypeRedirect(
                "custom_event2",
                new Uri("http://starwars.com/event/custom_event2"));
        }
    }
}
