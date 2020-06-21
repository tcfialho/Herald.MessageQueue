using System;

namespace Herald.MessageQueue.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TopicNameAttribute : Attribute
    {
        public string TopicName { get; }

        public TopicNameAttribute(string topic)
        {
            TopicName = topic;
        }
    }
}
