namespace RabbitSharp.Slack.Events
{
    /// <summary>
    /// Represents the mechanism used by the middleware to handle Slack events.
    /// </summary>
    public enum SlackEventHandling
    {
        /// <summary>
        /// Event hasn't been handled yet.
        /// </summary>
        None = 0,

        /// <summary>
        /// Handles the Slack event by responding with an HTTP redirect status code.
        /// </summary>
        Redirect = 1,

        /// <summary>
        /// Handles the Slack event by re-writing the request path.
        /// </summary>
        Rewrite = 2,

        /// <summary>
        /// Indicates the event has been handled and the response should end.
        /// </summary>
        EndResponse = 3
    }
}
