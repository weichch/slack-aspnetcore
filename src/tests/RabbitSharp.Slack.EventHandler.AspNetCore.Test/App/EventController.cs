using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace RabbitSharp.Slack.Events.Tests.App
{
    [Route("events")]
    public class EventController : Controller
    {
        [Route("{eventId}")]
        public async Task<IActionResult> HandleCustomEvent(
            string eventId,
            [FromBody] JObject evt)
        {
            await Task.Yield();
            return Ok(evt);
        }
    }
}
