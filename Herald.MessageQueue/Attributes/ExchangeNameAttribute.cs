using System;

namespace Herald.MessageQueue.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExchangeNameAttribute : Attribute
    {
        public string ExchangeName { get; }

        public ExchangeNameAttribute(string exchange)
        {
            ExchangeName = exchange;
        }
    }
}
