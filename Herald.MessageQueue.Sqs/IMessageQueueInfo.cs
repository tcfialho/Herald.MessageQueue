using System;

namespace Herald.MessageQueue.Sqs
{
    public interface IMessageQueueInfo
    {
        string GetQueueName(Type type);
        string GetQueueUrl(Type type);
    }
}
