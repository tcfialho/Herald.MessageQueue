using Herald.MessageQueue.AzureStorageQueue.Attributes;
using Herald.MessageQueue.Extensions;

using System;

namespace Herald.MessageQueue.AzureStorageQueue
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
            var queueName = type.GetAttribute<QueueNameAttribute>()?.QueueName ?? string.Concat(type.Name, _options.QueueNameSufix);
            return queueName.ToLower();
        }
    }
}
