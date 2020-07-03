using Herald.MessageQueue.Attributes;
using Herald.MessageQueue.Extensions;

using Microsoft.Extensions.Configuration;

using System;

namespace Herald.MessageQueue.Kafka
{
    public class MessageQueueInfo : IMessageQueueInfo
    {
        private readonly MessageQueueOptions _options;
        private readonly IConfiguration _configuration;

        public MessageQueueInfo(MessageQueueOptions options, IConfiguration configuration)
        {
            _options = options;
            _configuration = configuration;
        }

        public string GetTopicName(Type type)
        {
            var configuredName = _configuration[string.Concat(type.Name, "Topic")];

            if (!string.IsNullOrWhiteSpace(configuredName))
            {
                return configuredName;
            }

            var attributeName = type.GetAttribute<TopicNameAttribute>()?.TopicName;

            if (!string.IsNullOrWhiteSpace(attributeName))
            {
                return attributeName;
            }

            return string.Concat(type.Name, _options.TopicNameSufix);
        }
    }
}
