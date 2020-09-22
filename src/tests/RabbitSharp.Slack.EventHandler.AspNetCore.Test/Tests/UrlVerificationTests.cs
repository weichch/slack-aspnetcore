using System;
using System.Net;
using System.Threading.Tasks;
using RabbitSharp.Slack.Events.Tests.App;
using Xunit;

namespace RabbitSharp.Slack.Events.Tests.Tests
{
    public class UrlVerificationTests : AppTests
    {
        [Fact]
        public async Task ShouldHandleUrlVerificationRequest()
        {
            await Host.StartAsync();

            var challenge = Guid.NewGuid().ToString("N");
            var request = CreateSlackEventRequest("slack/event-hub", CreateUrlVerificationBody(challenge));
            var response = await TestClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType!.MediaType);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(challenge, content);
        }

        [Fact]
        public async Task ShouldNotHandleEventRequest()
        {
            await Host.StartAsync();

            var request = CreateSlackEventRequest("slack/event-hub", CreateEventBody());
            var response = await TestClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.MisdirectedRequest, response.StatusCode);
        }

        protected override void ConfigureSlackEventHandler(SlackEventHandlerOptions options)
        {
            options.AddUrlVerification();
        }
    }
}
