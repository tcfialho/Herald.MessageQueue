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

                var consumerConfig = new ConsumerConfig();
                consumerConfig.BootstrapServers = config.Host;
                consumerConfig.GroupId = config.GroupId;

                consumerConfig.MaxPollIntervalMs = config.MaxPollIntervalMs;
                consumerConfig.AutoCommitIntervalMs = config.AutoCommitIntervalMs;
                consumerConfig.Acks = config.Acks;

                consumerConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
                consumerConfig.EnableAutoCommit = true;
                consumerConfig.EnableAutoOffsetStore = false;

                consumerConfig.SecurityProtocol = config.SecurityProtocol;
                consumerConfig.SslEndpointIdentificationAlgorithm = config.SslEndpointIdentificationAlgorithm;
                consumerConfig.SaslMechanism = config.SaslMechanism;
                consumerConfig.SaslUsername = config.SaslUsername;
                consumerConfig.SaslPassword = config.SaslPassword;

                var factory = new ConsumerBuilder<Ignore, string>(consumerConfig);

                return factory.Build();
            });

            services.TryAddSingleton(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<MessageQueueOptions>();

                var producerConfig = new ProducerConfig();
                producerConfig.BootstrapServers = config.Host;

                producerConfig.SecurityProtocol = config.SecurityProtocol;
                producerConfig.SslEndpointIdentificationAlgorithm = config.SslEndpointIdentificationAlgorithm;
                producerConfig.SaslMechanism = config.SaslMechanism;
                producerConfig.SaslUsername = config.SaslUsername;
                producerConfig.SaslPassword = config.SaslPassword;

                var factory = new ProducerBuilder<Null, string>(producerConfig);
                return factory.Build();
            });

            return new MessageQueueBuilder(services);
        }
    }
}
