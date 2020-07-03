using System;

namespace Herald.MessageQueue.AzureStorageQueue
{
    public interface IMessageQueueInfo
    {
        string GetQueueName(Type type);
    }
}