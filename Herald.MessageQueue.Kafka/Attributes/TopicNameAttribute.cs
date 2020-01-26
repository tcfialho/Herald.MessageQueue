using System;

namespace Herald.MessageQueue.Kafka.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TopicNameAttribute : Attribute
    {
        public string TopicName { get; }

        public TopicNameAttribute(string topicName)
        {
            TopicName = topicName;
        }
    }
}
