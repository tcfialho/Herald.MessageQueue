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
        private readonly MessageQueueOptions _options;
        private readonly ITopicInfo _topicInfo;

        public MessageQueueKafka(IConsumer<Ignore, string> consumer,
                                 IProducer<Null, string> producer,
                                 MessageQueueOptions options,
                                 ITopicInfo topicInfo)
        {
            _consumer = consumer;
            _producer = producer;
            _options = options;
            _topicInfo = topicInfo;
        }

        public Task Received(MessageBase message)
        {
            _consumer.StoreOffset((ConsumeResult<Ignore, string>)message.QueueData);

            return Task.CompletedTask;
        }

        public async Task Send(MessageBase message)
        {
            var queueName = _topicInfo.GetTopicName(message.GetType());

            var messageBody = JsonConvert.SerializeObject(message);

            await _producer.ProduceAsync(queueName, new Message<Null, string> { Value = messageBody });
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>(int maxNumberOfMessages) where TMessage : MessageBase
        {
            if (maxNumberOfMessages < 1)
            {
                throw new ArgumentException("Max number of messages should be greater than zero.");
            }

            var queueName = _topicInfo.GetTopicName(typeof(TMessage));

            if (!_consumer.Subscription.Contains(queueName))
            {
                _consumer.Subscribe(queueName);
            }

            for (var i = 0; i < maxNumberOfMessages; i++)
            {
                var result = _consumer.Consume(TimeSpan.FromSeconds(5));

                if (result != null)
                {
                    var obj = JsonConvert.DeserializeObject<TMessage>(result.Message.Value);

                    obj.QueueData = result;

                    yield return await Task.FromResult(obj);
                }
            }
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>([EnumeratorCancellation] CancellationToken cancellationToken) where TMessage : MessageBase
        {
            var queueName = _topicInfo.GetTopicName(typeof(TMessage));

            if (!_consumer.Subscription.Contains(queueName))
            {
                _consumer.Subscribe(queueName);
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await Task.Run(() => _consumer.Consume(cancellationToken), cancellationToken).DefaultIfCanceled();

                TMessage message = null;

                if (result != null)
                {
                    message = JsonConvert.DeserializeObject<TMessage>(result.Message.Value);

                    message.QueueData = result;
                }

                if (message == null)
                {
                    continue;
                }

                yield return await Task.FromResult(message);
            }
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
