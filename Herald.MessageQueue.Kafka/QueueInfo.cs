using Herald.MessageQueue.Extensions;
using Herald.MessageQueue.Kafka.Attributes;

using System;

namespace Herald.MessageQueue.Kafka
{
    public class QueueInfo : IQueueInfo
    {
        private readonly MessageQueueOptions _options;

        public QueueInfo(MessageQueueOptions options)
        {
            _options = options;
        }

        public string GetQueueName(Type type)
        {
            return type.GetAttribute<TopicNameAttribute>()?.TopicName ?? string.Concat(type.Name, _options.TopicNameSufix);
        }
    }
}
