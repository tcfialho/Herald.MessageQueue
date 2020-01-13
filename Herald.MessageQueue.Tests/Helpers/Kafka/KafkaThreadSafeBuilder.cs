
using Herald.MessageQueue.Kafka;

using Microsoft.Extensions.DependencyInjection;

using System;

namespace Herald.MessageQueue.Tests
{
    public static class KafkaThreadSafeBuilder
    {
        private static readonly object syncRoot = new Object();
        public static IMessageQueue Build()
        {
            IMessageQueue queue;
            lock (syncRoot)
            {
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddMessageQueueKafka(setup =>
                {
                    setup.Host = "localhost";
                    setup.Port = "9092";
                    setup.GroupId = nameof(TestMessage);
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();

                queue = serviceProvider.GetService<IMessageQueue>();
            }
            return queue;
        }
    }
}
