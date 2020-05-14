
using Herald.MessageQueue.Kafka;

using Microsoft.Extensions.DependencyInjection;

namespace Herald.MessageQueue.Tests.Helpers.Kafka
{
    public static class KafkaThreadSafeBuilder
    {
        private static readonly object syncRoot = new object();
        public static IMessageQueue Build()
        {
            IMessageQueue queue;
            lock (syncRoot)
            {
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddMessageQueueKafka(setup =>
                {
                    setup.Host = "localhost:9092";
                    setup.GroupId = nameof(TestMessage);
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();

                queue = serviceProvider.GetService<IMessageQueue>();
            }
            return queue;
        }
    }
}
