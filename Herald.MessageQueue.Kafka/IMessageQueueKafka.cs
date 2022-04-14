using System.Threading.Tasks;

namespace Herald.MessageQueue.Kafka
{
    public interface IMessageQueueKafka : IMessageQueue
    {
        Task Send<TMessage>(TMessage message, string topicName) where TMessage : MessageBase;
    }
}