using System;

namespace Herald.MessageQueue.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RoutingKeyAttribute : Attribute
    {
        public string RoutingKey { get; }

        public RoutingKeyAttribute(string routing)
        {
            RoutingKey = routing;
        }
    }
}
