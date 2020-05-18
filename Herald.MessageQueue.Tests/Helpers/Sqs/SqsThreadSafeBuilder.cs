
using Herald.MessageQueue.Sqs;

using Microsoft.Extensions.DependencyInjection;

namespace Herald.MessageQueue.Tests.Helpers.Sqs
{
    public static class SqsThreadSafeBuilder
    {
        private static readonly object _syncRoot = new object();
        public static IMessageQueue Build()
        {
            IMessageQueue queue;
            lock (_syncRoot)
            {
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddMessageQueueSqs(setup =>
                {
                    setup.ServiceURL = "http://localhost:4576";
                    setup.GroupId = nameof(TestMessage);
                    setup.Region = "us-east-1";
                    setup.VisibilityTimeout = 1;
                    setup.EnableFifo = true;
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();

                queue = serviceProvider.GetService<IMessageQueue>();
            }
            return queue;
        }
    }
}
