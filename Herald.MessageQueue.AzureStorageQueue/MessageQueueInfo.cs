using Herald.MessageQueue.AzureStorageQueue.Attributes;
using Herald.MessageQueue.Extensions;

using System;

namespace Herald.MessageQueue.AzureStorageQueue
{
    public class MessageQueueInfo : IMessageQueueInfo
    {
        private readonly MessageQueueOptions _options;

        public MessageQueueInfo(MessageQueueOptions options)
        {
            _options = options;
        }

        public string GetQueueName(Type type)
        {
            var queueName = type.GetAttribute<QueueNameAttribute>()?.QueueName ?? string.Concat(type.Name, _options.QueueNameSufix);
            return queueName.ToLower();
        }
    }
}
