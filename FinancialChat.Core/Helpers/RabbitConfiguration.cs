namespace FinancialChat.Core.Helpers
{
    public sealed class RabbitConfiguration
    {
        public string HostName { get; set; }
        public string QueueName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ListenToQueueName { get; set; }
    }
}
