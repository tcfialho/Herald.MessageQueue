namespace Herald.MessageQueue.Kafka
{
    public class MessageQueueOptions
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string GroupId { get; set; }
        public object TopicNameSufix { get; set; } = "Topic";
        public int MaxPollIntervalMs { get; set; } = 10 * 1000;
        public int AutoCommitIntervalMs { get; set; } = 500;
    }
}
