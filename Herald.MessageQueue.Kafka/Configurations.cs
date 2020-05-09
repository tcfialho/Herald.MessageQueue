using Confluent.Kafka;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System;

namespace Herald.MessageQueue.Kafka
{
    public static class Configurations
    {
        public static IMessageQueueBuilder AddMessageQueueKafka(this IServiceCollection services, Action<MessageQueueOptions> options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            services.Configure(options);
            var messageQueueOptions = new MessageQueueOptions();
            options?.Invoke(messageQueueOptions);

            services.TryAddSingleton(messageQueueOptions);

            services.TryAddSingleton<IMessageQueue, MessageQueueKafka>();

            services.TryAddSingleton<IMessageQueueInfo, MessageQueueInfo>();

            services.TryAddSingleton(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<MessageQueueOptions>();

                var factory = new ConsumerBuilder<Ignore, string>(new ConsumerConfig
                {
                    GroupId = config.GroupId,
                    BootstrapServers = $"{config.Host}:{config.Port}",
                    MaxPollIntervalMs = config.MaxPollIntervalMs,
                    AutoCommitIntervalMs = config.AutoCommitIntervalMs,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = true,
                    EnableAutoOffsetStore = false
                });

                return factory.Build();
            });

            services.TryAddSingleton(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<MessageQueueOptions>();

                var factory = new ProducerBuilder<Null, string>(new ProducerConfig
                {
                    BootstrapServers = $"{config.Host}:{config.Port}"
                });

                return factory.Build();
            });

            return new MessageQueueBuilder(services);
        }
    }
}
