using Herald.MessageQueue.Extensions;
using Herald.MessageQueue.RabbitMq.Attributes;

using System;

namespace Herald.MessageQueue.RabbitMq
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
            return type.GetAttribute<QueueNameAttribute>()?.QueueName ?? string.Concat(type.Name, _options.QueueNameSufix);
        }

        public string GetExchangeName(Type type)
        {
            return type.GetAttribute<ExchangeNameAttribute>()?.ExchangeName ?? string.Concat(type.Name, _options.ExchangeNameSufix);
        }
    }
}
