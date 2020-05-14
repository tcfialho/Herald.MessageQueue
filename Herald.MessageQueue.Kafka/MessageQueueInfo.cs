using System;

using Herald.MessageQueue.Extensions;
using Herald.MessageQueue.Kafka.Attributes;

namespace Herald.MessageQueue.Kafka
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
            return type.GetAttribute<TopicNameAttribute>()?.TopicName ?? string.Concat(type.Name, _options.TopicNameSufix);
        }
    }
}
