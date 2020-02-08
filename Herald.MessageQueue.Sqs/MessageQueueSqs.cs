using Amazon.SQS;
using Amazon.SQS.Model;

using Herald.MessageQueue.Extensions;

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

        private string GetQueueUrl(Type type)
        {
            return $"{_options.Host}:{_options.Port}/queue/{_queueInfo.GetQueueName(type)}.fifo";
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

        public async IAsyncEnumerable<TMessage> Receive<TMessage>(int maxNumberOfMessages) where TMessage : MessageBase
        {
            if (maxNumberOfMessages < 1)
                throw new ArgumentException("Max number of messages should be greater than zero.");

            if (maxNumberOfMessages > 10)
                throw new ArgumentException("Highest max number of messages avaliable in Sqs is 10.");

            var result = await _amazonSqs.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = GetQueueUrl(typeof(TMessage)),
                MaxNumberOfMessages = maxNumberOfMessages,
                WaitTimeSeconds = _options.WaitTimeSeconds,
                VisibilityTimeout = _options.VisibilityTimeout
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

            var config = new ReceiveMessageRequest
            {
                QueueUrl = GetQueueUrl(typeof(TMessage)),
                WaitTimeSeconds = _options.WaitTimeSeconds,
                MaxNumberOfMessages = 10
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await _amazonSqs.ReceiveMessageAsync(config, cancellationToken).DefaultIfCanceled();

                if (result == null)
                    continue;

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