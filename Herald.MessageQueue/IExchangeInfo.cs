using System;

namespace Herald.MessageQueue
{
    public interface IExchangeInfo
    {
        string GetExchangeName(Type type);
    }
}
