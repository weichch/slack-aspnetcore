using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RabbitSharp.Slack.Events
{
    public interface ISlackEventHandler
    {
        ValueTask HandleAsync(SlackEventContext context);
    }
}
