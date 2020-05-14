using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Herald.MessageQueue.Extensions;

using Microsoft.Azure.Storage.Queue;

using Newtonsoft.Json;

namespace Herald.MessageQueue.AzureStorageQueue
{
    public class MessageQueueAzureStorageQueue : IMessageQueue, IDisposable
    {
        private readonly CloudQueueClient _queueClient;
        private readonly MessageQueueOptions _options;
        private readonly IMessageQueueInfo _queueInfo;

        private CloudQueue _queue;

        public MessageQueueAzureStorageQueue(CloudQueueClient queueClient,
                                             MessageQueueOptions options,
                                             IMessageQueueInfo queueInfo)
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

            _queue = GetQueueReference(typeof(TMessage));

            for (var i = 0; i < maxNumberOfMessages; i++)
            {
                var result = await _queue.GetMessageAsync();

                if (result != null)
                {
                    var obj = JsonConvert.DeserializeObject<TMessage>(result.AsString);

                    obj.QueueData = result.PopReceipt;

                    yield return await Task.FromResult(obj);
                }
            }
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>([EnumeratorCancellation] CancellationToken cancellationToken = default) where TMessage : MessageBase
        {
            _queue = GetQueueReference(typeof(TMessage));

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await Task.Run(() => _queue.GetMessageAsync(cancellationToken), cancellationToken).DefaultIfCanceled();

                TMessage message = null;

                if (result != null)
                {
                    message = JsonConvert.DeserializeObject<TMessage>(result.AsString);

                    message.QueueData = result.PopReceipt;
                }

                if (message == null)
                {
                    continue;
                }

                yield return await Task.FromResult(message);
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
    }
}
