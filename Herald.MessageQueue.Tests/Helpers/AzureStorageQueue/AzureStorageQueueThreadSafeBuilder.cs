using Herald.MessageQueue.AzureStorageQueue;

using Microsoft.Extensions.DependencyInjection;

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
