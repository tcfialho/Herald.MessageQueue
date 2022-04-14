using System.Threading.Tasks;

namespace Herald.MessageQueue.RabbitMq
{
    public interface IMessageQueueRabbitMq : IMessageQueue
    {
        Task Send<TMessage>(TMessage message, string exchangeName, string routingKey) where TMessage : MessageBase;
    }
}