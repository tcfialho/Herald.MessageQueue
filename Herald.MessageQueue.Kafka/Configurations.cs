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
            services.TryAdd(new ServiceDescriptor(typeof(IMessageQueueKafka), typeof(MessageQueueKafka), serviceLifetime));
            services.TryAdd(new ServiceDescriptor(typeof(IMessageQueue), x => x.GetRequiredService<IMessageQueueKafka>(), serviceLifetime));
            services.TryAdd(new ServiceDescriptor(typeof(IMessageQueueInfo), typeof(MessageQueueInfo), serviceLifetime));
            services.TryAdd(new ServiceDescriptor(typeof(ConsumerConfig), serviceProvider => CreateConsumerConfig(serviceProvider.GetRequiredService<MessageQueueOptions>()), serviceLifetime));
            services.TryAdd(new ServiceDescriptor(typeof(ProducerConfig), serviceProvider => CreateProducerConfig(serviceProvider.GetRequiredService<MessageQueueOptions>()), serviceLifetime));

            services.TryAdd(new ServiceDescriptor(typeof(IConsumer<Ignore, string>), serviceProvider =>
            {
                var consumerConfig = serviceProvider.GetRequiredService<ConsumerConfig>();
                var factory = new ConsumerBuilder<Ignore, string>(consumerConfig);                
                return factory.Build();
            }, serviceLifetime));

            services.TryAdd(new ServiceDescriptor(typeof(IProducer<Null, string>), serviceProvider =>
            {
                var producerConfig = serviceProvider.GetRequiredService<ProducerConfig>();
                var factory = new ProducerBuilder<Null, string>(producerConfig);
                return factory.Build();
            }, serviceLifetime));

            return new MessageQueueBuilder(services);
        }

        private static ProducerConfig CreateProducerConfig(MessageQueueOptions options) => new ProducerConfig
        {
            BootstrapServers = options.BootstrapServers,
            SecurityProtocol = options.SecurityProtocol,
            SslEndpointIdentificationAlgorithm = options.SslEndpointIdentificationAlgorithm,
            SaslMechanism = options.SaslMechanism,
            SaslUsername = options.SaslUsername,
            SaslPassword = options.SaslPassword
        };

        private static ConsumerConfig CreateConsumerConfig(MessageQueueOptions options) => new ConsumerConfig
        {
            BootstrapServers = options.BootstrapServers,
            GroupId = options.GroupId,
            SessionTimeoutMs = options.SessionTimeoutMs,
            MaxPollIntervalMs = options.MaxPollIntervalMs,
            AutoCommitIntervalMs = options.AutoCommitIntervalMs,
            Acks = options.Acks,

            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            EnableAutoOffsetStore = false,

            SecurityProtocol = options.SecurityProtocol,
            SslEndpointIdentificationAlgorithm = options.SslEndpointIdentificationAlgorithm,
            SaslMechanism = options.SaslMechanism,
            SaslUsername = options.SaslUsername,
            SaslPassword = options.SaslPassword
        };
    }
}
