using Azure;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace Herald.MessageQueue.AzureStorageQueue
{
    //public class QueueClient2
    //{
    //    private readonly QueueClient _client;

    //    public QueueClient2(string connectionString, string queueName)
    //    {
    //        _client = new QueueClient(connectionString, queueName);
    //    }

    //    public virtual async Task<Response<SendReceipt>> SendMessageAsync(string messageText)
    //    {
    //        return await _client.SendMessageAsync(messageText);
    //    }
    //}

    public class QueueClientFactory : IQueueClientFactory
    {
        public QueueClient Create(string connectionString, string queueName)
        {
            return new QueueClient(connectionString, queueName);
        }
    }
}
