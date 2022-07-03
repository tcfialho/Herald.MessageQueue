
using Herald.MessageQueue.Sqs;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Herald.MessageQueue.Tests.Helpers.Sqs
{
    public class SqsThreadSafeBuilder : IThreadSafeBuilder
    {
        private static readonly object _syncRoot = new object();
        public IMessageQueue Build()
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

                serviceCollection.AddMessageQueueSqs(setup =>
                {
                    setup.ServiceURL = "http://localhost:4576";
                    setup.GroupId = Guid.NewGuid().ToString();
                    setup.Region = "us-east-1";
                    setup.VisibilityTimeout = 1;
                    setup.EnableFifo = true;
                    setup.WaitTimeSeconds = 0;
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();

                queue = serviceProvider.GetService<IMessageQueue>();
            }
            return queue;
        }
    }
}
