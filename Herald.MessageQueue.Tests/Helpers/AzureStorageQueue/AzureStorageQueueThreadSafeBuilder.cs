using Herald.MessageQueue.AzureStorageQueue;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Collections.Generic;

namespace Herald.MessageQueue.Tests.Helpers.RabbitMq
{
    public static class AzureStorageQueueThreadSafeBuilder
    {
        private static readonly object syncRoot = new object();
        public static IMessageQueue Build()
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

                serviceCollection.AddMessageQueueAzureStorageQueue(setup =>
                {
                    setup.ConnectionString = "UseDevelopmentStorage=true";
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();

                queue = serviceProvider.GetService<IMessageQueue>();
            }
            return queue;
        }
    }
}
