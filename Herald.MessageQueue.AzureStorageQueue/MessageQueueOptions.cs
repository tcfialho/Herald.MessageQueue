namespace Herald.MessageQueue.AzureStorageQueue
{
    public class MessageQueueOptions
    {
        public string ConnectionString { get; set; }

        public string QueueNameSufix { get; set; }
    }
}
