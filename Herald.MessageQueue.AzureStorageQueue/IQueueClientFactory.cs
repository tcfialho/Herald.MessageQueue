using Azure.Storage.Queues;

namespace Herald.MessageQueue.AzureStorageQueue
{
    public interface IQueueClientFactory
    {
        QueueClient Create(string connectionString, string queueName);
    }
}
