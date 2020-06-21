using Herald.MessageQueue.Attributes;
using Herald.MessageQueue.Extensions;

using Microsoft.Extensions.Configuration;

using System;

namespace Herald.MessageQueue.RabbitMq
{
    public class RoutingKeyInfo : IRoutingKeyInfo
    {
        private readonly MessageQueueOptions _options;
        private readonly IConfiguration _configuration;

        public RoutingKeyInfo(MessageQueueOptions options, IConfiguration configuration)
        {
            _options = options;
            _configuration = configuration;
        }

        public string GetRoutingKey(Type type)
        {
            var configuredName = _configuration[string.Concat(type.Name, "RoutingKey")];

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
