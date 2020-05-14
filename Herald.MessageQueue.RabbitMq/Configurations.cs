using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using RabbitMQ.Client;

namespace Herald.MessageQueue.RabbitMq
{
    public static class Configurations
    {
        public static IMessageQueueBuilder AddMessageQueueRabbitMq(this IServiceCollection services, Action<MessageQueueOptions> options)
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

            services.TryAddSingleton<IConnection>(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<MessageQueueOptions>();
                var factory = new ConnectionFactory()
                {
                    HostName = config.Host,
                    Port = int.Parse(config.Port),
                    UserName = config.Username,
                    Password = config.Password,
                    VirtualHost = config.VirtualHost,
                    DispatchConsumersAsync = true
                };
                return factory.CreateConnection();
            });

            services.TryAddSingleton<IModel>(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<MessageQueueOptions>();
                var connection = serviceProvider.GetRequiredService<IConnection>();
                var channel = connection.CreateModel();

                channel.ConfirmSelect();
                channel.WaitForConfirmsOrDie();
                channel.BasicQos(0, 1, false);

                return channel;
            });

            services.TryAddSingleton<IMessageQueue, MessageQueueRabbitMq>();

            services.TryAddSingleton<IMessageQueueInfo, MessageQueueInfo>();

            return new MessageQueueBuilder(services);
        }
    }
}
