using Herald.MessageQueue.Extensions;
using Herald.MessageQueue.RabbitMq.Attributes;

using System;

namespace Herald.MessageQueue.RabbitMq
{
    public class ExchangeInfo : IExchangeInfo
    {
        private readonly MessageQueueOptions _options;

        public ExchangeInfo(MessageQueueOptions options)
        {
            _options = options;
        }

        public string GetExchangeName(Type type)
        {
            return type.GetAttribute<ExchangeNameAttribute>()?.ExchangeName ?? string.Concat(type.Name, _options.ExchangeNameSufix);
        }
    }
}
