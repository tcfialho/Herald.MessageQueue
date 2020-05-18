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

            services.TryAddSingleton<IMessageQueue, MessageQueueKafka>();

            services.TryAddSingleton<IQueueInfo, QueueInfo>();

            services.TryAddSingleton(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<MessageQueueOptions>();

                var consumerConfig = new ConsumerConfig
                {
                    BootstrapServers = config.BootstrapServers,
                    GroupId = config.GroupId,

                    MaxPollIntervalMs = config.MaxPollIntervalMs,
                    AutoCommitIntervalMs = config.AutoCommitIntervalMs,
                    Acks = config.Acks,

                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = true,
                    EnableAutoOffsetStore = false,

                    SecurityProtocol = config.SecurityProtocol,
                    SslEndpointIdentificationAlgorithm = config.SslEndpointIdentificationAlgorithm,
                    SaslMechanism = config.SaslMechanism,
                    SaslUsername = config.SaslUsername,
                    SaslPassword = config.SaslPassword
                };

                var factory = new ConsumerBuilder<Ignore, string>(consumerConfig);

                return factory.Build();
            });

            services.TryAddSingleton(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<MessageQueueOptions>();

                var producerConfig = new ProducerConfig
                {
                    BootstrapServers = config.BootstrapServers,

                    SecurityProtocol = config.SecurityProtocol,
                    SslEndpointIdentificationAlgorithm = config.SslEndpointIdentificationAlgorithm,
                    SaslMechanism = config.SaslMechanism,
                    SaslUsername = config.SaslUsername,
                    SaslPassword = config.SaslPassword
                };

                var factory = new ProducerBuilder<Null, string>(producerConfig);
                return factory.Build();
            });

            return new MessageQueueBuilder(services);
        }
    }
}
