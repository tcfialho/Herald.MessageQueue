using Confluent.Kafka;

namespace Herald.MessageQueue.Kafka
{
    public class MessageQueueOptions
    {
        public string BootstrapServers { get; set; }
        public string GroupId { get; set; }

        public int MaxPollIntervalMs { get; set; } = 10 * 1000;
        public int AutoCommitIntervalMs { get; set; } = 500;

        public Acks? Acks { get; set; }

        public string SaslUsername { get; set; }
        public string SaslPassword { get; set; }
        public SaslMechanism? SaslMechanism { get; set; }
        public SecurityProtocol? SecurityProtocol { get; set; }
        public SslEndpointIdentificationAlgorithm? SslEndpointIdentificationAlgorithm { get; set; }

        public string TopicNameSufix { get; set; } = "Topic";
        public int RequestDelaySeconds { get; set; } = 1;
    }
}
