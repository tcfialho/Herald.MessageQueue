namespace Herald.MessageQueue.AzureStorageQueue
{
    public class MessageQueueOptions
    {
        public string ConnectionString { get; set; }

        public string QueueNameSufix { get; set; } = "Queue";

        public int RequestDelaySeconds { get; set; } = 1;
        public string ConfigSection { get; set; }
    }
}
