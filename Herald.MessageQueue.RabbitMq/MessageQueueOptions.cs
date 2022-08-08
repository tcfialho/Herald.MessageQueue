using System.Collections.Generic;

namespace Herald.MessageQueue.RabbitMq
{
    public class MessageQueueOptions
    {
        public string HostName { get; set; }
        public string Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; } = "/";
        public string QueueNameSufix { get; set; } = "Queue";
        public string ExchangeNameSufix { get; set; } = "Exchange";
        public int RequestDelaySeconds { get; set; } = 1;
        public int AutoNackTimeoutSeconds { get; set; } = 30;
        public string ConfigSection { get; set; }
    }
}
