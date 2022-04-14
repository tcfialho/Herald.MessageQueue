using System.Threading.Tasks;

namespace Herald.MessageQueue.Sqs
{
    public interface IMessageQueueSqs : IMessageQueue
    {
        Task Send<TMessage>(TMessage message, string queueName) where TMessage : MessageBase;
    }
}