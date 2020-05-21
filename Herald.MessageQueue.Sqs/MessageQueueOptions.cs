namespace Herald.MessageQueue.Sqs
{
    public class MessageQueueOptions
    {
        public string ServiceURL { get; set; }
        public string GroupId { get; set; }
        public string Region { get; set; }
        public int WaitTimeSeconds { get; set; } = 20;
        public int VisibilityTimeout { get; set; } = 30;
        public string QueueNameSufix { get; set; } = "Queue";
        public bool EnableFifo { get; set; }
    }
}