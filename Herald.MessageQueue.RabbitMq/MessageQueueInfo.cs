using System;

using Herald.MessageQueue.Extensions;
using Herald.MessageQueue.RabbitMq.Attributes;

namespace Herald.MessageQueue.RabbitMq
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
            return type.GetAttribute<QueueNameAttribute>()?.QueueName ?? string.Concat(type.Name, _options.QueueNameSufix);
        }
    }
}
