using System;

namespace Herald.MessageQueue.RabbitMq.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExchangeNameAttribute : Attribute
    {
        public string ExchangeName { get; }

        public ExchangeNameAttribute(string exchangeName)
        {
            ExchangeName = exchangeName;
        }
    }
}
