using Herald.MessageQueue.Attributes;
using Herald.MessageQueue.Extensions;

using Microsoft.Extensions.Configuration;

using System;

namespace Herald.MessageQueue.RabbitMq
{
    public class ExchangeInfo : IExchangeInfo
    {
        private readonly MessageQueueOptions _options;
        private readonly IConfiguration _configuration;

        public ExchangeInfo(MessageQueueOptions options, IConfiguration configuration)
        {
            _options = options;
            _configuration = configuration;
        }

        public string GetExchangeName(Type type)
        {
            var configuredName = _configuration[string.Concat(type.Name, "Exchange")];

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
    }
}
