﻿using Herald.MessageQueue.Extensions;

using Microsoft.Azure.Storage.Queue;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IMessageQueueInfo _info;

        private CloudQueue _queue;

        public MessageQueueAzureStorageQueue(CloudQueueClient queueClient,
                                             MessageQueueOptions options,
                                             IMessageQueueInfo info)
        {
            _queueClient = queueClient;
            _options = options;
            _info = info;
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
                throw new ArgumentException("Max number of messages should not be greater than 32.");
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

                yield return message;
            }
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>([EnumeratorCancellation] CancellationToken cancellationToken = default) where TMessage : MessageBase
        {
            _queue = GetQueueReference(typeof(TMessage));

            const int maxNumberOfMessages = 10;

            while (!cancellationToken.IsCancellationRequested)
            {
                var results = await _queue.GetMessagesAsync(maxNumberOfMessages, cancellationToken).DefaultIfCanceled();

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
            var queueName = _info.GetQueueName(type);

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
