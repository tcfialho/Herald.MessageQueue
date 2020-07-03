using System;

namespace Herald.MessageQueue.Kafka
{
    public interface IMessageQueueInfo
    {
        string GetTopicName(Type type);
    }
}