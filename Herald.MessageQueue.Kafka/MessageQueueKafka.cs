using Confluent.Kafka;

using Herald.MessageQueue.Extensions;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue.Kafka
{
    public class MessageQueueKafka : IMessageQueue, IDisposable
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IProducer<Null, string> _producer;
        private readonly MessageQueueOptions _kafkaConfig;

        public MessageQueueKafka(IConsumer<Ignore, string> consumer,
                                 IProducer<Null, string> producer,
                                 MessageQueueOptions rabbitMQConfig)
        {
            _consumer = consumer;
            _producer = producer;
            _kafkaConfig = rabbitMQConfig;
        }

        public Task Received(MessageBase message)
        {
            _consumer.StoreOffset((ConsumeResult<Ignore, string>)message.QueueData);

            return Task.CompletedTask;
        }

        public async Task Send(MessageBase message)
        {
            var queueName = GetQueueName(message.GetType());

            var messageBody = JsonConvert.SerializeObject(message);

            await _producer.ProduceAsync(queueName, new Message<Null, string> { Value = messageBody });
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>(int maxNumberOfMessages) where TMessage : MessageBase
        {
            if (maxNumberOfMessages < 1)
                throw new ArgumentException("Max number of messages should be greater than zero.");

            var queueName = GetQueueName(typeof(TMessage));

            if (!_consumer.Subscription.Contains(queueName))
                _consumer.Subscribe(queueName);

            for (int i = 0; i < maxNumberOfMessages; i++)
            {
                var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(5));

                if (consumeResult != null)
                {
                    var obj = JsonConvert.DeserializeObject<TMessage>(consumeResult.Value);

                    obj.QueueData = consumeResult;

                    yield return await Task.FromResult(obj);
                }
            }
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>([EnumeratorCancellation] CancellationToken cancellationToken) where TMessage : MessageBase
        {
            var queueName = GetQueueName(typeof(TMessage));

            if (!_consumer.Subscription.Contains(queueName))
                _consumer.Subscribe(queueName);


            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await Task.Run(() => _consumer.Consume(cancellationToken), cancellationToken).DefaultIfCanceled();

                TMessage message = null;

                if (result != null)
                {
                    message = JsonConvert.DeserializeObject<TMessage>(result.Value);

                    message.QueueData = result;
                }

                if (message == null)
                    continue;

                yield return await Task.FromResult(message);
            }
        }

        private string GetQueueName(Type type)
        {
            return type.Name;
        }

        public void Dispose()
        {
            _consumer?.Close();
            _consumer?.Dispose();
            _producer?.Dispose();
        }
    }
}
