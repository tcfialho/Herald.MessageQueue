
using Herald.MessageQueue.RabbitMq;

using Microsoft.Extensions.DependencyInjection;

namespace Herald.MessageQueue.Tests.Helpers.RabbitMq
{
    public static class RabbitMqThreadSafeBuilder
    {
        private static readonly object syncRoot = new object();
        public static IMessageQueue Build(string exchangeName)
        {
            IMessageQueue queue;
            lock (syncRoot)
            {
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddMessageQueueRabbitMq(setup =>
                {
                    setup.HostName = "localhost";
                    setup.Port = "5672";
                    setup.UserName = "user";
                    setup.Password = "password";
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();

                queue = serviceProvider.GetService<IMessageQueue>();
            }
            return queue;
        }
    }
}
