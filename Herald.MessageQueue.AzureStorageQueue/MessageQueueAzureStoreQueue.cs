using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Herald.MessageQueue.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue.AzureStorageQueue
{

    public class MessageQueueAzureStorageQueue : IMessageQueueAzureStorageQueue, IDisposable
    {
        private readonly IQueueClientFactory _queueClientFactory;
        private readonly MessageQueueOptions _options;
        private readonly IMessageQueueInfo _info;

        public MessageQueueAzureStorageQueue(IQueueClientFactory queueClientFactory,
                                             MessageQueueOptions options,
                                             IMessageQueueInfo info)
        {
            _queueClientFactory = queueClientFactory;
            _options = options;
            _info = info;
        }

        public void Dispose()
        {
            
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>(int maxNumberOfMessages) where TMessage : MessageBase
        {
            if (maxNumberOfMessages < 1)
            {
                throw new ArgumentException("Max number of messages should be greater than zero.");
            }

            if (maxNumberOfMessages > 32)
            {
                throw new ArgumentException("Max number of messages should not be greater than 32.");
            }

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var i = 0;
            await foreach (var message in Receive<TMessage>(cancellationToken, maxNumberOfMessages))
            {
                if (i >= maxNumberOfMessages)
                    break;
                i++;
                yield return message;
            }
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>([EnumeratorCancellation] CancellationToken cancellationToken = default) where TMessage : MessageBase
        {
            const int maxNumberOfMessages = 1;

            await foreach (var message in Receive<TMessage>(cancellationToken, maxNumberOfMessages))
            {
                yield return message;
            }
        }

        private async IAsyncEnumerable<TMessage> Receive<TMessage>([EnumeratorCancellation] CancellationToken cancellationToken, int maxNumberOfMessages) where TMessage : MessageBase
        {
            var queueName = _info.GetQueueName(typeof(TMessage));
            var queueClient = _queueClientFactory.Create(_options.ConnectionString, queueName);

            while (!cancellationToken.IsCancellationRequested)
            {
                var results = (QueueMessage[])await queueClient.ReceiveMessagesAsync(maxNumberOfMessages, default, cancellationToken).DefaultIfCanceled();

                if (results == null || !results.Any())
                {
                    cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(_options.RequestDelaySeconds));
                    continue;
                }

                foreach (var item in results)
                {
                    var message = ReceiveMessage<TMessage>(item);

                    if (message == null)
                    {
                        continue;
                    }

                    yield return message;
                }
            }
        }

        public async Task Received<TMessage>(TMessage message) where TMessage : MessageBase
        {
            var queueName = _info.GetQueueName(message.GetType());
            var queueClient = _queueClientFactory.Create(_options.ConnectionString, queueName);
            var queueData = ((string MessageId, string PopReceipt))message.QueueData;
            await queueClient.DeleteMessageAsync(queueData.MessageId, queueData.PopReceipt);
        }

        public async Task Send<TMessage>(TMessage message) where TMessage : MessageBase
        {
            var queueName = _info.GetQueueName(message.GetType());
            await Send<TMessage>(message, queueName);
        }

        public async Task Send<TMessage>(TMessage message, string destination) where TMessage : MessageBase
        {
            var queueClient = _queueClientFactory.Create(_options.ConnectionString, destination);

            var messageBody = JsonSerializer.Serialize(message, message.GetType());
            var body = Encoding.UTF8.GetBytes(messageBody);

            await queueClient.SendMessageAsync(messageBody);
        }

        private TMessage ReceiveMessage<TMessage>(QueueMessage message) where TMessage : MessageBase
        {
            TMessage obj = null;

            if (message != null)
            {
                var body = message.Body.ToString();
                obj = JsonSerializer.Deserialize<TMessage>(body);
                obj.QueueData = CreateQueueData(message);
            }

            return obj;
        }

        private static (string MessageId, string PopReceipt) CreateQueueData(QueueMessage message)
        {
            (string MessageId, string PopReceipt) queueData;
            queueData.MessageId = message.MessageId;
            queueData.PopReceipt = message.PopReceipt;            
            return queueData;
        }
    }
}
