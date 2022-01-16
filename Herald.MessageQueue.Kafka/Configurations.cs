using Confluent.Kafka;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System;

namespace Herald.MessageQueue.Kafka
{
    public static class Configurations
    {
        public static IMessageQueueBuilder AddMessageQueueKafka(this IServiceCollection services, Action<MessageQueueOptions> options, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
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
            services.TryAdd(new ServiceDescriptor(typeof(IMessageQueue), typeof(MessageQueueKafka), serviceLifetime));
            services.TryAdd(new ServiceDescriptor(typeof(IMessageQueueInfo), typeof(MessageQueueInfo), serviceLifetime));

            services.TryAdd(new ServiceDescriptor(typeof(IConsumer<Ignore, string>), serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<MessageQueueOptions>();
                var consumerConfig = new ConsumerConfig
                {
                    BootstrapServers = config.BootstrapServers,
                    GroupId = config.GroupId,

                    SessionTimeoutMs = config.SessionTimeoutMs,
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
            }, serviceLifetime));

            services.TryAdd(new ServiceDescriptor(typeof(IProducer<Null, string>), serviceProvider =>
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
            }, serviceLifetime));

            return new MessageQueueBuilder(services);
        }
    }
}
