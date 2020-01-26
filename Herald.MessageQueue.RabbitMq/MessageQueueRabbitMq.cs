using Herald.MessageQueue.Extensions;
using Herald.MessageQueue.RabbitMq.Attributes;

using Newtonsoft.Json;

using RabbitMQ.Client;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue.RabbitMq
{
    public class MessageQueueRabbitMq : IMessageQueue, IDisposable
    {
        private readonly IModel _channel;
        private readonly IConnection _connection;
        private readonly MessageQueueOptions _options;

        public MessageQueueRabbitMq(IModel channel,
                                    IConnection connection,
                                    MessageQueueOptions options)
        {
            _channel = channel;
            _connection = connection;
            _options = options;
        }

        public Task Received(MessageBase message)
        {
            _channel.BasicAck((ulong)message.QueueData, false);

            return Task.CompletedTask;
        }

        public Task Send(MessageBase message)
        {
            var messageBody = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(messageBody);

            var exchangeName = GetExchangeName(message);
            var routingKey = GetRoutingKeyName(message);

            _channel.BasicPublish(exchangeName, routingKey, null, body);

            return Task.CompletedTask;
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>(int maxNumberOfMessages) where TMessage : MessageBase
        {
            if (maxNumberOfMessages < 1)
                throw new ArgumentException("Max number of messages should be greater than zero.");

            var queueName = GetQueueName(typeof(TMessage));

            for (int i = 0; i < 5; i++)
            {
                var message = ReceiveMessage<TMessage>(queueName);

                if (message == null)
                    continue;

                yield return await Task.FromResult(message);
            }
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>([EnumeratorCancellation] CancellationToken cancellationToken) where TMessage : MessageBase
        {
            var queueName = GetQueueName(typeof(TMessage));

            while (!cancellationToken.IsCancellationRequested)
            {
                var message = ReceiveMessage<TMessage>(queueName);

                if (message == null)
                    continue;

                yield return await Task.FromResult(message);
            }
        }

        private TMessage ReceiveMessage<TMessage>(string queueName) where TMessage : MessageBase
        {
            TMessage obj = null;
            var message = _channel.BasicGet(queueName, false);

            if (message != null)
            {
                var body = Encoding.UTF8.GetString(message.Body);
                obj = JsonConvert.DeserializeObject<TMessage>(body);
                obj.QueueData = message.DeliveryTag;
            }

            return obj;
        }

        private string GetQueueName(Type type)
        {
            return type.GetAttribute<QueueNameAttribute>()?.QueueName ?? string.Concat(type.Name, _options.QueueNameSufix);
        }

        private string GetExchangeName(MessageBase message)
        {
            return message.GetAttribute<ExchangeNameAttribute>()?.ExchangeName ?? string.Concat(message.GetType().Name, _options.ExchangeNameSufix);
        }

        private string GetRoutingKeyName(MessageBase message)
        {
            return message.GetAttribute<RoutingKeyAttribute>()?.RoutingKey ?? string.Empty;
        }

        public void Dispose()
        {
            if (_channel.IsOpen)
                _channel.Close();

            if (_connection.IsOpen)
                _connection.Close();

            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
