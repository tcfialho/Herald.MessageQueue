using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System;

namespace Herald.MessageQueue.AzureStorageQueue
{
    public static class Configurations
    {
        public static IMessageQueueBuilder AddMessageQueueAzureStorageQueue(this IServiceCollection services, Action<MessageQueueOptions> options, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
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

            services.TryAdd(new ServiceDescriptor(typeof(MessageQueueOptions), x => messageQueueOptions, serviceLifetime));
            services.TryAdd(new ServiceDescriptor(typeof(IMessageQueueAzureStorageQueue), typeof(MessageQueueAzureStorageQueue), serviceLifetime));
            services.TryAdd(new ServiceDescriptor(typeof(IMessageQueue), x => x.GetRequiredService<IMessageQueueAzureStorageQueue>(), serviceLifetime));
            services.TryAdd(new ServiceDescriptor(typeof(IMessageQueueInfo), typeof(MessageQueueInfo), serviceLifetime));
            services.TryAdd(new ServiceDescriptor(typeof(IQueueClientFactory), typeof(QueueClientFactory), serviceLifetime));

            return new MessageQueueBuilder(services);
        }
    }
}
