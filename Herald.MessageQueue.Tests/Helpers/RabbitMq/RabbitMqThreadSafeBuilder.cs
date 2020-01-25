
using Herald.MessageQueue.RabbitMq;

using Microsoft.Extensions.DependencyInjection;

using System;

namespace Herald.MessageQueue.Tests
{
    public static class RabbitMqThreadSafeBuilder
    {
        private static readonly object syncRoot = new Object();
        public static IMessageQueue Build(string exchangeName)
        {
            IMessageQueue queue;
            lock (syncRoot)
            {
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddMessageQueueRabbitMq(setup =>
                {
                    setup.ExchangeName = exchangeName;
                    setup.Host = "localhost";
                    setup.Port = "5672";
                    setup.Username = "user";
                    setup.Password = "password";
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();

                queue = serviceProvider.GetService<IMessageQueue>();
            }
            return queue;
        }
    }
}
