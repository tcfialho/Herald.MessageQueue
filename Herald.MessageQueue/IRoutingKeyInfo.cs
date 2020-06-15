using System;

namespace Herald.MessageQueue
{
    public interface IRoutingKeyInfo
    {
        string GetRoutingKey(Type type);
    }
}
