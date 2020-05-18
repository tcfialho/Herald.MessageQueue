using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System;

namespace Herald.MessageQueue.AzureStorageQueue
{
    public static class Configurations
    {
        public static IMessageQueueBuilder AddMessageQueueAzureStorageQueue(this IServiceCollection services, Action<MessageQueueOptions> options)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            services.Configure(options);
            var messageQueueOptions = new MessageQueueOptions();
            options?.Invoke(messageQueueOptions);

            services.TryAddSingleton(messageQueueOptions);

            services.TryAddSingleton<CloudQueueClient>(serviceProvider =>
            {
                var storageAccount = CloudStorageAccount.Parse(messageQueueOptions.ConnectionString);
                var queueClient = storageAccount.CreateCloudQueueClient();
                return queueClient;
            });

            services.TryAddSingleton<IMessageQueue, MessageQueueAzureStorageQueue>();

            services.TryAddSingleton<IMessageQueueInfo, MessageQueueInfo>();

            return new MessageQueueBuilder(services);
        }
    }
}
