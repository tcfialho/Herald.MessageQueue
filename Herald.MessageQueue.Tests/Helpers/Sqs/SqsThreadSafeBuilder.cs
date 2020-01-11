﻿
using Herald.MessageQueue.Sqs;

using Microsoft.Extensions.DependencyInjection;

using System;

namespace Herald.MessageQueue.Tests
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
                    setup.GroupId = "test";
                    setup.RegionEndpoint = "us-east-1";
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();

                queue = serviceProvider.GetService<IMessageQueue>();
            }
            return queue;
        }
    }
}
