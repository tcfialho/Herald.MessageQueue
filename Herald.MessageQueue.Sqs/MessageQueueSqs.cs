﻿using Amazon.SQS;
using Amazon.SQS.Model;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue.Sqs
{
    public class MessageQueueSqs : IMessageQueue, IDisposable
    {
        private readonly IAmazonSQS _amazonSqs;
        private readonly MessageQueueOptions _options;

        public MessageQueueSqs(IAmazonSQS amazonSQS,
                               IOptions<MessageQueueOptions> options)
        {
            _options = options.Value;
            _amazonSqs = amazonSQS;
        }

        private string GetQueueUrl(Type type)
        {
            return $"{_options.Host}:{_options.Port}/queue/{type.Name}.fifo";
        }

        public async Task Send(MessageBase @message)
        {
            await _amazonSqs.SendMessageAsync(new SendMessageRequest
            {
                QueueUrl = GetQueueUrl(@message.GetType()),
                MessageDeduplicationId = Guid.NewGuid().ToString(),
                MessageGroupId = _options.GroupId,
                MessageBody = JsonConvert.SerializeObject(@message),
            });
        }

        public async Task Received(MessageBase @message)
        {
            await _amazonSqs.DeleteMessageAsync(new DeleteMessageRequest
            {
                QueueUrl = GetQueueUrl(@message.GetType()),
                ReceiptHandle = (string)@message.QueueData
            });
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>() where TMessage : MessageBase
        {
            var result = await _amazonSqs.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = GetQueueUrl(typeof(TMessage)),
                MaxNumberOfMessages = 5,
                WaitTimeSeconds = 5,
                VisibilityTimeout = 30
            });

            foreach (var item in result.Messages)
            {
                var messageBody = JsonConvert.DeserializeObject<TMessage>(item.Body);

                messageBody.QueueData = item.ReceiptHandle;

                yield return messageBody;
            }
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>([EnumeratorCancellation] CancellationToken cancellationToken) where TMessage : MessageBase
        {
            var queueUrl = GetQueueUrl(typeof(TMessage));

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await _amazonSqs.ReceiveMessageAsync(queueUrl, cancellationToken);

                foreach (var item in result.Messages)
                {
                    var messageBody = JsonConvert.DeserializeObject<TMessage>(item.Body);

                    messageBody.QueueData = item.ReceiptHandle;

                    yield return messageBody;
                }
            }
        }

        public void Dispose()
        {
            _amazonSqs?.Dispose();
        }
    }
}