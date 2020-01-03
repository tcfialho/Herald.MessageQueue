namespace Herald.MessageQueue.Kafka
{
    public class MessageQueueOptions
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string GroupId { get; set; }
    }
}
