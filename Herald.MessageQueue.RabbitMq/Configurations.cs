using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using RabbitMQ.Client;

using System;

namespace Herald.MessageQueue.RabbitMq
{
    public static class Configurations
    {
        public static IMessageQueueBuilder AddMessageQueueRabbitMq(this IServiceCollection services, Action<MessageQueueOptions> options, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
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
            services.TryAdd(new ServiceDescriptor(typeof(IMessageQueueRabbitMq), typeof(MessageQueueRabbitMq), serviceLifetime));
            services.TryAdd(new ServiceDescriptor(typeof(IMessageQueue), x => x.GetRequiredService<IMessageQueueRabbitMq>(), serviceLifetime));
            services.TryAdd(new ServiceDescriptor(typeof(IMessageQueueInfo), typeof(MessageQueueInfo), serviceLifetime));

            services.TryAdd(new ServiceDescriptor(typeof(IConnection), serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<MessageQueueOptions>();
                var factory = new ConnectionFactory()
                {
                    HostName = config.HostName,
                    Port = int.Parse(config.Port),
                    UserName = config.UserName,
                    Password = config.Password,
                    VirtualHost = config.VirtualHost,
                    DispatchConsumersAsync = true
                };
                return factory.CreateConnection();
            }, serviceLifetime));

            return new MessageQueueBuilder(services);
        }
    }
}
