using Herald.MessageQueue.Attributes;
using Herald.MessageQueue.Extensions;

using Microsoft.Extensions.Configuration;

using System;

namespace Herald.MessageQueue.RabbitMq
{
    public class QueueInfo : IQueueInfo
    {
        private readonly MessageQueueOptions _options;
        private readonly IConfiguration _configuration;

        public QueueInfo(MessageQueueOptions options, IConfiguration configuration)
        {
            _options = options;
            _configuration = configuration;
        }

        public string GetQueueName(Type type)
        {
            var configuredName = _configuration[string.Concat(type.Name, "Queue")];

            if (!string.IsNullOrWhiteSpace(configuredName))
            {
                return configuredName;
            }

            var attributeName = type.GetAttribute<QueueNameAttribute>()?.QueueName;

            if (!string.IsNullOrWhiteSpace(attributeName))
            {
                return attributeName;
            }

            return string.Concat(type.Name, _options.QueueNameSufix);
        }
    }
}
