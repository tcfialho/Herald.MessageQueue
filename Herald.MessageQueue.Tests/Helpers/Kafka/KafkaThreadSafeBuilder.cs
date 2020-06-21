
using Herald.MessageQueue.Kafka;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Collections.Generic;

namespace Herald.MessageQueue.Tests.Helpers.Kafka
{
    public static class KafkaThreadSafeBuilder
    {
        private static readonly object _syncRoot = new object();
        public static IMessageQueue Build()
        {
            IMessageQueue queue;
            lock (_syncRoot)
            {
                var serviceCollection = new ServiceCollection();

                var configuration = (IConfiguration)new ConfigurationBuilder()
                    .AddInMemoryCollection(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("KEY", "VALUE"),
                    })
                    .Build();

                serviceCollection.AddSingleton(configuration);

                serviceCollection.AddMessageQueueKafka(setup =>
                {
                    setup.BootstrapServers = "localhost:9092";
                    setup.GroupId = nameof(TestMessage);
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();

                queue = serviceProvider.GetService<IMessageQueue>();
            }
            return queue;
        }
    }
}
