using Herald.MessageQueue.Attributes;
using Herald.MessageQueue.Extensions;

using Microsoft.Extensions.Configuration;

using System;

namespace Herald.MessageQueue.RabbitMq
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

        public string GetExchangeName(Type type)
        {            
            var configuredName = _configuration[string.Concat(_options.ConfigSection, ":", type.Name, ":", "Exchange")];

            if (!string.IsNullOrWhiteSpace(configuredName))
            {
                return configuredName;
            }

            var attributeName = type.GetAttribute<ExchangeNameAttribute>()?.ExchangeName;

            if (!string.IsNullOrWhiteSpace(attributeName))
            {
                return attributeName;
            }

            return string.Concat(type.Name, _options.ExchangeNameSufix);
        }

        public string GetQueueName(Type type)
        {
            var configuredName = _configuration[string.Concat(_options.ConfigSection, ":", type.Name, ":", "Queue")];

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

        public string GetRoutingKey(Type type)
        {
            var configuredName = _configuration[string.Concat(_options.ConfigSection, ":", type.Name, ":", "RoutingKey")];

            if (!string.IsNullOrWhiteSpace(configuredName))
            {
                return configuredName;
            }

            var attributeName = type.GetAttribute<RoutingKeyAttribute>()?.RoutingKey;

            if (!string.IsNullOrWhiteSpace(attributeName))
            {
                return attributeName;
            }

            return string.Empty;
        }
    }
}
