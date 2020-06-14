using Herald.MessageQueue.Extensions;
using Herald.MessageQueue.Attributes;

using System;

namespace Herald.MessageQueue.Kafka
{
    public class TopicInfo : ITopicInfo
    {
        private readonly MessageQueueOptions _options;

        public TopicInfo(MessageQueueOptions options)
        {
            _options = options;
        }

        public string GetTopicName(Type type)
        {
            return type.GetAttribute<TopicNameAttribute>()?.TopicName ?? string.Concat(type.Name, _options.TopicNameSufix);
        }
    }
}
