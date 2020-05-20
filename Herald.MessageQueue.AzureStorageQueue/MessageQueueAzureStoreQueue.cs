using Herald.MessageQueue.Extensions;

using Microsoft.Azure.Storage.Queue;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue.AzureStorageQueue
{
    public class MessageQueueAzureStorageQueue : IMessageQueue, IDisposable
    {
        private readonly CloudQueueClient _queueClient;
        private readonly MessageQueueOptions _options;
        private readonly IQueueInfo _queueInfo;

        private CloudQueue _queue;

        public MessageQueueAzureStorageQueue(CloudQueueClient queueClient,
                                             MessageQueueOptions options,
                                             IQueueInfo queueInfo)
        {
            _queueClient = queueClient;
            _options = options;
            _queueInfo = queueInfo;
        }

        public void Dispose()
        {
            _queue = null;
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>(int maxNumberOfMessages) where TMessage : MessageBase
        {
            if (maxNumberOfMessages < 1)
            {
                throw new ArgumentException("Max number of messages should be greater than zero.");
            }

            if (maxNumberOfMessages > 32)
            {
                throw new ArgumentException("Highest max number of messages avaliable in azure storage queue is 32.");
            }

            _queue = GetQueueReference(typeof(TMessage));

            var messages = await _queue.GetMessagesAsync(maxNumberOfMessages);

            foreach (var item in messages)
            {
                var message = ReceiveMessage<TMessage>(item);

                if (message == null)
                {
                    continue;
                }

                yield return await Task.FromResult(message);
            }
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>([EnumeratorCancellation] CancellationToken cancellationToken = default) where TMessage : MessageBase
        {
            _queue = GetQueueReference(typeof(TMessage));

            const int maxNumberOfMessages = 10;

            while (!cancellationToken.IsCancellationRequested)
            {
                var results = await Task.Run(() => _queue.GetMessagesAsync(maxNumberOfMessages, cancellationToken), cancellationToken).DefaultIfCanceled();

                if (results == null)
                {
                    cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(_options.WaitTimeSeconds));
                    continue;
                }

                foreach (var item in results)
                {
                    var message = ReceiveMessage<TMessage>(item);

                    if (message == null)
                    {
                        continue;
                    }

                    yield return await Task.FromResult(message);
                }
            }
        }

        public async Task Received(MessageBase message)
        {
            _queue = GetQueueReference(message.GetType());

            await _queue.DeleteMessageAsync(new CloudQueueMessage("", message.QueueData.ToString()));
        }

        public async Task Send(MessageBase message)
        {
            var messageBody = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(messageBody);

            _queue = GetQueueReference(message.GetType());

            await _queue.AddMessageAsync(new CloudQueueMessage(body));
        }

        private CloudQueue GetQueueReference(Type type)
        {
            var queueName = _queueInfo.GetQueueName(type);

            if (_queue == null || _queue.Name != queueName)
            {
                _queue = _queueClient.GetQueueReference(queueName);
            }

            return _queue;
        }

        private TMessage ReceiveMessage<TMessage>(CloudQueueMessage message) where TMessage : MessageBase
        {
            TMessage obj = null;

            if (message != null)
            {
                var body = message.AsString;
                obj = JsonConvert.DeserializeObject<TMessage>(body);
                obj.QueueData = message.PopReceipt;
            }

            return obj;
        }
    }
}
