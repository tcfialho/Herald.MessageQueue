using Herald.MessageQueue.Extensions;
using Herald.MessageQueue.Sqs.Attributes;

using System;

namespace Herald.MessageQueue.Sqs
{
    public class MessageQueueInfo : IMessageQueueInfo
    {
        private readonly MessageQueueOptions _options;

        public MessageQueueInfo(MessageQueueOptions options)
        {
            _options = options;
        }

        public string GetQueueName(Type type)
        {
            return type.GetAttribute<QueueNameAttribute>()?.QueueName ?? type.Name;
        }
    }
}
