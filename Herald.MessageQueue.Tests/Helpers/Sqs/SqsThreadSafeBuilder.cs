
using Herald.MessageQueue.Sqs;

using Microsoft.Extensions.DependencyInjection;

using System;

namespace Herald.MessageQueue.Tests.Helpers.Sqs
{
    public static class SqsThreadSafeBuilder
    {
        private static readonly object syncRoot = new Object();
        public static IMessageQueue Build()
        {
            IMessageQueue queue;
            lock (syncRoot)
            {
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddMessageQueueSqs(setup =>
                {
                    setup.Host = "http://127.0.0.1";
                    setup.Port = "4576";
                    setup.GroupId = nameof(TestMessage);
                    setup.RegionEndpoint = "us-east-1";
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
