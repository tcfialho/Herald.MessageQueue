using System;

namespace Herald.MessageQueue
{
    public interface ITopicInfo
    {
        string GetTopicName(Type type);
    }
}
