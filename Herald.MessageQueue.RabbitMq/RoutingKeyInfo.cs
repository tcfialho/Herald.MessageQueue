using Herald.MessageQueue.Attributes;
using Herald.MessageQueue.Extensions;

using System;

namespace Herald.MessageQueue.RabbitMq
{
    public class RoutingKeyInfo : IRoutingKeyInfo
    {
        private readonly MessageQueueOptions _options;

        public RoutingKeyInfo(MessageQueueOptions options)
        {
            _options = options;
        }

        public string GetRoutingKey(Type type)
        {
            return type.GetAttribute<RoutingKeyAttribute>()?.RoutingKey ?? string.Empty;
        }
    }
}
