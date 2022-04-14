using Confluent.Kafka;

using Herald.MessageQueue.Extensions;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue.Kafka
{
    public class MessageQueueKafka : IMessageQueueKafka, IDisposable
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IProducer<Null, string> _producer;
        private readonly MessageQueueOptions _options;
        private readonly IMessageQueueInfo _info;

        public MessageQueueKafka(IConsumer<Ignore, string> consumer,
                                 IProducer<Null, string> producer,
                                 MessageQueueOptions options,
                                 IMessageQueueInfo info)
        {
            _consumer = consumer;
            _producer = producer;
            _options = options;
            _info = info;
        }

        public Task Received(MessageBase message)
        {
            _consumer.StoreOffset((ConsumeResult<Ignore, string>)message.QueueData);

            return Task.CompletedTask;
        }

        public async Task Send<TMessage>(TMessage message) where TMessage : MessageBase
        {
            await Send(message, _info.GetTopicName(message.GetType()));
        }

        public async Task Send<TMessage>(TMessage message, string topicName) where TMessage : MessageBase
        {
            var messageBody = JsonSerializer.Serialize(message);

            await _producer.ProduceAsync(topicName, new Message<Null, string> { Value = messageBody });
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>(int maxNumberOfMessages) where TMessage : MessageBase
        {
            if (maxNumberOfMessages < 1)
            {
                throw new ArgumentException("Max number of messages should be greater than zero.");
            }

            var queueName = _info.GetTopicName(typeof(TMessage));

            if (!_consumer.Subscription.Contains(queueName))
            {
                _consumer.Subscribe(queueName);
            }

            for (var i = 0; i < maxNumberOfMessages; i++)
            {
                var result = _consumer.Consume(TimeSpan.FromSeconds(5));

                if (result != null)
                {
                    var obj = JsonSerializer.Deserialize<TMessage>(result.Message.Value);

                    obj.QueueData = result;

                    yield return await Task.FromResult(obj);
                }
            }
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>(TimeSpan timeout) where TMessage : MessageBase
        {
            var queueName = _info.GetTopicName(typeof(TMessage));

            if (!_consumer.Subscription.Contains(queueName))
            {
                _consumer.Subscribe(queueName);
            }

            var cancellationToken = new CancellationTokenSource(timeout).Token;

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(timeout);

                var message = ReceiveMessage<TMessage>(result);

                if (message == null)
                {
                    cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(_options.RequestDelaySeconds));
                    continue;
                }

                yield return await Task.FromResult(message);
            }
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>([EnumeratorCancellation] CancellationToken cancellationToken) where TMessage : MessageBase
        {
            var queueName = _info.GetTopicName(typeof(TMessage));

            if (!_consumer.Subscription.Contains(queueName))
            {
                _consumer.Subscribe(queueName);
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await Task.Run(() => _consumer.Consume(cancellationToken), cancellationToken).DefaultIfCanceled();

                var message = ReceiveMessage<TMessage>(result);

                if (message == null)
                {
                    cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(_options.RequestDelaySeconds));
                    continue;
                }

                yield return message;
            }
        }

        private TMessage ReceiveMessage<TMessage>(ConsumeResult<Ignore, string> result) where TMessage : MessageBase
        {
            TMessage message = null;

            if (result != null)
            {
                var body = result.Message.Value;
                message = JsonSerializer.Deserialize<TMessage>(body);
                message.QueueData = result;
            }

            return message;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _consumer?.Close();
                _consumer?.Dispose();
                _producer?.Dispose();
            }
        }
    }
}
