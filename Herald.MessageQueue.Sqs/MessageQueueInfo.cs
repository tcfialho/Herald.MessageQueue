using Herald.MessageQueue.Attributes;
using Herald.MessageQueue.Extensions;

using Microsoft.Extensions.Configuration;

using System;

namespace Herald.MessageQueue.Sqs
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

        public string GetQueueUrl(Type type)
        {
            var queueUrl = string.Empty;

            if (string.IsNullOrEmpty(_options.ServiceURL))
            {
                queueUrl = $"/{GetQueueName(type)}{(_options.EnableFifo ? ".fifo" : "")}";
            }
            else
            {
                queueUrl = $"{_options.ServiceURL}/queue/{GetQueueName(type)}{(_options.EnableFifo ? ".fifo" : "")}";
            }

            return queueUrl;
        }
    }
}
