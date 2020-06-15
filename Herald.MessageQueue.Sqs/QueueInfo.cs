using Herald.MessageQueue.Attributes;
using Herald.MessageQueue.Extensions;

using System;

namespace Herald.MessageQueue.Sqs
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
    }
}
