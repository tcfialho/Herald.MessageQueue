using Amazon.SQS;
using Amazon.SQS.Model;

using Herald.MessageQueue.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue.Sqs
{
    public class MessageQueueSqs : IMessageQueueSqs, IDisposable
    {
        private readonly IAmazonSQS _amazonSqs;
        private readonly IMessageQueueInfo _queueInfo;
        private readonly MessageQueueOptions _options;

        public MessageQueueSqs(IAmazonSQS amazonSQS,
                               MessageQueueOptions options,
                               IMessageQueueInfo queueInfo)
        {
            _amazonSqs = amazonSQS;
            _options = options;
            _queueInfo = queueInfo;
        }

        public async Task Send<TMessage>(TMessage message) where TMessage : MessageBase
        {
            await Send(message, _queueInfo.GetQueueName(@message.GetType()));
        }

        public async Task Send<TMessage>(TMessage message, string destination) where TMessage : MessageBase
        {
            await _amazonSqs.SendMessageAsync(new SendMessageRequest
            {
                QueueUrl = _queueInfo.GetQueueUrl(destination),
                MessageDeduplicationId = _options.EnableFifo ? Guid.NewGuid().ToString() : null,
                MessageGroupId = _options.GroupId,
                MessageBody = JsonSerializer.Serialize(message, message.GetType())
        });
        }

        public async Task Received<TMessage>(TMessage message) where TMessage : MessageBase
        {
            await _amazonSqs.DeleteMessageAsync(new DeleteMessageRequest
            {
                QueueUrl = _queueInfo.GetQueueUrl(@message.GetType()),
                ReceiptHandle = (string)@message.QueueData
            });
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>(int maxNumberOfMessages) where TMessage : MessageBase
        {
            if (maxNumberOfMessages < 1)
            {
                throw new ArgumentException("Max number of messages should be greater than zero.");
            }

            if (maxNumberOfMessages > 10)
            {
                throw new ArgumentException("Max number of messages should not be greater than 10.");
            }

            var result = await _amazonSqs.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = _queueInfo.GetQueueUrl(typeof(TMessage)),
                MaxNumberOfMessages = maxNumberOfMessages,
                WaitTimeSeconds = _options.WaitTimeSeconds,
                VisibilityTimeout = _options.VisibilityTimeout
            });

            foreach (var item in result.Messages)
            {
                var message = ReceiveMessage<TMessage>(item);

                if (message == null)
                {
                    continue;
                }

                yield return message;
            }
        }

        public async IAsyncEnumerable<TMessage> Receive<TMessage>([EnumeratorCancellation] CancellationToken cancellationToken) where TMessage : MessageBase
        {
            var config = new ReceiveMessageRequest
            {
                QueueUrl = _queueInfo.GetQueueUrl(typeof(TMessage)),
                WaitTimeSeconds = _options.WaitTimeSeconds,
                MaxNumberOfMessages = 10
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await _amazonSqs.ReceiveMessageAsync(config, cancellationToken).DefaultIfCanceled();

                if (result == null || !result.Messages.Any())
                {
                    cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(_options.RequestDelaySeconds));
                    continue;
                }

                foreach (var item in result.Messages)
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

        private TMessage ReceiveMessage<TMessage>(Message result) where TMessage : MessageBase
        {
            TMessage message = null;

            if (result != null)
            {
                var body = result.Body;
                message = JsonSerializer.Deserialize<TMessage>(body);
                message.QueueData = result.ReceiptHandle;
            }

            return message;
        }

        public void Dispose()
        {
            _amazonSqs?.Dispose();
        }
    }
}