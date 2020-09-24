using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace SimpleEventHandler.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SlackEventsController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post([FromBody] JObject e)
        {
            return Ok();
        }
    }
}
