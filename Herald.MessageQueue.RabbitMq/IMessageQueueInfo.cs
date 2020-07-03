using System;

namespace Herald.MessageQueue.RabbitMq
{
    public interface IMessageQueueInfo
    {
        string GetExchangeName(Type type);
        string GetQueueName(Type type);
        string GetRoutingKey(Type type);
    }
}