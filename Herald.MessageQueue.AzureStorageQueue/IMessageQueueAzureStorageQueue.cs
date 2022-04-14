using System.Threading.Tasks;

namespace Herald.MessageQueue.AzureStorageQueue
{
    public interface IMessageQueueAzureStorageQueue : IMessageQueue
    {
        Task Send<TMessage>(TMessage message, string queueName) where TMessage : MessageBase;
    }
}