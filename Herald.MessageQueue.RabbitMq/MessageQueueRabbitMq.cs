
using Newtonsoft.Json;

using RabbitMQ.Client;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Herald.MessageQueue.RabbitMq
{
    public class MessageQueueRabbitMq : IMessageQueue, IDisposable
    {
        private readonly IModel _channel;
        private readonly IConnection _connection;
        private readonly MessageQueueOptions _rabbitMQConfig;

        public MessageQueueRabbitMq(IModel channel,
                                    IConnection connection,
                                    MessageQueueOptions rabbitMQConfig)
        {
            _channel = channel;
            _connection = connection;
            _rabbitMQConfig = rabbitMQConfig;
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

            _channel.BasicPublish(_rabbitMQConfig.ExchangeName, "", null, body);

            return Task.CompletedTask;
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>() where TMessage : MessageBase
        {
            var queueName = GetQueueName(typeof(TMessage));

            for (int i = 0; i < 5; i++)
            {
                var message = _channel.BasicGet(queueName, false);

                if (message == null)
                {
                    await Task.Delay(1000);
                    continue;
                }

                var body = Encoding.UTF8.GetString(message.Body);
                var obj = JsonConvert.DeserializeObject<TMessage>(body);
                obj.QueueData = message.DeliveryTag;

                yield return await Task.FromResult(obj);
            }
        }

        private string GetQueueName(Type type)
        {
            return type.Name;
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
