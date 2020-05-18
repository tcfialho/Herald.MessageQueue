using System;

namespace Herald.MessageQueue
{
    public interface IQueueInfo
    {
        string GetQueueName(Type type);
    }
}
