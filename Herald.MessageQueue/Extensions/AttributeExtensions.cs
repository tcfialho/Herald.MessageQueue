using System;
using System.Linq;

namespace Herald.MessageQueue.Extensions
{
    public static class AttributeExtensions
    {
        public static T GetAttribute<T>(this MessageBase instance) where T : Attribute
        {
            return instance.GetType().GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
        }

        public static T GetAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
        }
    }
}
