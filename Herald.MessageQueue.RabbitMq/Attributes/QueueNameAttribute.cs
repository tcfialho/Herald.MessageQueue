using System;

namespace Herald.MessageQueue.RabbitMq.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class QueueNameAttribute : Attribute
    {
        public string QueueName { get; }

        public QueueNameAttribute(string queueName)
        {
            QueueName = queueName;
        }
    }
}
