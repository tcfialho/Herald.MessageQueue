using System;

namespace Herald.MessageQueue
{
    public interface IMessageQueueInfo
    {
        string GetQueueName(Type type);
    }
}
