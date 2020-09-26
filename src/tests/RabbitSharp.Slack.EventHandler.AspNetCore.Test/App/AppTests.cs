using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitSharp.Slack.Http;

namespace RabbitSharp.Slack.Events.Tests.App
{
    /// <summary>
    /// Base class of all app tests.
    /// </summary>
    public abstract class AppTests : IDisposable
    {
        private IHost? _host;
        private HttpClient? _testClient;

        protected AppTests()
        {
            SigningSecret = Guid.NewGuid().ToString("N");

            HostBuilder = new HostBuilder();
            HostBuilder.ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices((context, services) => 
                    ConfigureAppServices(services, context.Configuration));
                webHost.Configure((context, app) =>
                    ConfigureApp(app, context.HostingEnvironment, context.Configuration));
            });
        }

        protected HostBuilder HostBuilder { get; }
        protected IHost Host => _host ??= HostBuilder.Build();
        protected HttpClient TestClient => _testClient ??= Host.GetTestClient();
        protected string SigningSecret { get; }

        protected virtual void ConfigureAppServices(
            IServiceCollection services, 
            IConfiguration configuration)
        {
            services.AddControllers().AddNewtonsoftJson();
        }

        protected virtual void ConfigureApp(
            IApplicationBuilder app,
            IHostEnvironment env,
            IConfiguration configuration)
        {
            var slackEventHandlerOptions = new SlackEventHandlerOptions
            {
                CallbackPath = "/slack/event-hub",
                RequestValidationParameters = new SlackRequestValidationParameters
                {
                    SigningSecretProvider = _ => SigningSecret
                }
            };
            ConfigureSlackEventHandler(slackEventHandlerOptions);

            app.UseSlackEventHandler(slackEventHandlerOptions);
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                ConfigureEndpoints(endpoints);
            });
        }

        protected abstract void ConfigureSlackEventHandler(SlackEventHandlerOptions options);

        protected virtual void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
        {
        }

        protected HttpRequestMessage CreateSlackEventRequest(string url, object body)
        {
            var content = JsonSerializer.Serialize(body, body.GetType());
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var baseString = $"v0:{timestamp:D}:{content}";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SigningSecret));
            var bodyHash = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(baseString)))
                .Replace("-", string.Empty)
                .ToLowerInvariant();

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("X-Slack-Request-Timestamp", timestamp.ToString("D"));
            request.Headers.Add("X-Slack-Signature", $"v0={bodyHash}");
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");

            return request;
        }

        public static object CreateUrlVerificationBody(string? challenge = null)
        {
            return new
            {
                token = Guid.NewGuid().ToString("N"),
                challenge = challenge ?? Guid.NewGuid().ToString("N"),
                type = "url_verification"
            };
        }

        public static object CreateEventBody(string? eventType = null)
        {
            return new
            {
                token = RandomString(),
                team_id = RandomString(),
                enterprise_id = RandomString(),
                api_app_id = RandomString(),
                @event = new
                {
                    client_msg_id = RandomString(),
                    type = eventType ?? RandomString(),
                    text = RandomString(),
                    user = RandomString(),
                    ts = RandomString(),
                    team = RandomString(),
                    blocks = new[]
                    {
                        new
                        {
                            type = RandomString(),
                            block_id = RandomString(),
                            elements = new[]
                            {
                                new
                                {
                                    type = RandomString(),
                                    elements = new[]
                                    {
                                        new
                                        {
                                            type = RandomString(),
                                            text = RandomString()
                                        }
                                    }
                                }
                            }
                        }
                    },
                    channel = RandomString(),
                    event_ts = RandomString(),
                    channel_type = RandomString()
                },
                type = RandomString(),
                event_id = RandomString(),
                event_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                authed_users = new[]
                {
                    RandomString(),
                    RandomString(),
                    RandomString(),
                }
            };

            static string RandomString()
            {
                return Guid.NewGuid().ToString("N");
            }
        }

        public virtual void Dispose()
        {
            _host?.Dispose();
        }
    }
}
