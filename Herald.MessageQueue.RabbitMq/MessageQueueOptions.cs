﻿namespace Herald.MessageQueue.RabbitMq
{
    public class MessageQueueOptions
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string ExchangeName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; } = "/";
    }
}
