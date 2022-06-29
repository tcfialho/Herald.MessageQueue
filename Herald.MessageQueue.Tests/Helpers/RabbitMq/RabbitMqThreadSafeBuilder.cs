
using Herald.MessageQueue.RabbitMq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Collections.Generic;

namespace Herald.MessageQueue.Tests.Helpers.RabbitMq
{
    public class RabbitMqThreadSafeBuilder : IThreadSafeBuilder
    {
        private static readonly object syncRoot = new object();
        public IMessageQueue Build()
        {
            IMessageQueue queue;
            lock (syncRoot)
            {
                var serviceCollection = new ServiceCollection();

                var configuration = (IConfiguration)new ConfigurationBuilder()
                    .AddInMemoryCollection(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("KEY", "VALUE"),
                    })
                    .Build();

                serviceCollection.AddSingleton(configuration);

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
