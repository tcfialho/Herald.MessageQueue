
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
        private readonly IMessageQueueInfo _info;

        public MessageQueueRabbitMq(IConnection connection,
                                    MessageQueueOptions options,
                                    IMessageQueueInfo info)
        {
            _channel = connection.CreateModel();
            _connection = connection;
            _options = options;
            _info = info;
        }

        public Task Received(MessageBase message)
        {
            var queueData = ((ulong DeliveryTag, CancellationTokenSource CancellationTokenSource))message.QueueData;
            queueData.CancellationTokenSource.Cancel();
            _channel.BasicAck(queueData.DeliveryTag, false);
            return Task.CompletedTask;
        }

        public Task Send(MessageBase message)
        {
            var messageType = message.GetType();

            var messageBody = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(messageBody);

            var exchangeName = _info.GetExchangeName(messageType);
            var routingKey = _info.GetRoutingKey(messageType);

            _channel.BasicPublish(exchangeName, routingKey, null, body);

            return Task.CompletedTask;
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>(int maxNumberOfMessages) where TMessage : MessageBase
        {
            if (maxNumberOfMessages < 1)
            {
                throw new ArgumentException("Max number of messages should be greater than zero.");
            }

            var queueName = _info.GetQueueName(typeof(TMessage));

            for (var i = 0; i < maxNumberOfMessages; i++)
            {
                var result = _channel.BasicGet(queueName, false);

                var message = ReceiveMessage<TMessage>(result);

                if (message == null)
                {
                    continue;
                }

                yield return await Task.FromResult(message);
            }
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>([EnumeratorCancellation] CancellationToken cancellationToken) where TMessage : MessageBase
        {
            var queueName = _info.GetQueueName(typeof(TMessage));

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = _channel.BasicGet(queueName, false);

                var message = ReceiveMessage<TMessage>(result);

                if (message == null)
                {
                    cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(_options.RequestDelaySeconds));
                    continue;
                }

                yield return await Task.FromResult(message);
            }
        }

        private static (ulong DeliveryTag, CancellationTokenSource CancellationTokenSource) CreateQueueData(ulong deliveryTag, CancellationTokenSource cts)
        {
            (ulong DeliveryTag, CancellationTokenSource CancellationTokenSource) queueData;
            queueData.DeliveryTag = deliveryTag;
            queueData.CancellationTokenSource = cts;
            return queueData;
        }

        private TMessage ReceiveMessage<TMessage>(BasicGetResult result) where TMessage : MessageBase
        {
            TMessage message = null;

            if (result != null)
            {
                var cts = new CancellationTokenSource();
                var body = Encoding.UTF8.GetString(result.Body.Span);
                message = JsonConvert.DeserializeObject<TMessage>(body);
                message.QueueData = CreateQueueData(result.DeliveryTag, cts);
                Task.Delay(TimeSpan.FromSeconds(_options.AutoNackTimeoutSeconds)).ContinueWith(task =>
                {
                    _channel.BasicNack(result.DeliveryTag, false, true);
                }, cts.Token).ConfigureAwait(false);
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
                if (_channel.IsOpen)
                {
                    _channel.Close();
                }

                if (_connection.IsOpen)
                {
                    _connection.Close();
                }

                _channel?.Dispose();
                _connection?.Dispose();
            }
        }
    }
}
