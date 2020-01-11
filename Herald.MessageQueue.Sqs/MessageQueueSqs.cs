using Amazon.SQS;
using Amazon.SQS.Model;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var queueUrl = $"{_options.Host}:{_options.Port}/queue/{type.Name.ToLower()}.fifo";

            Trace.WriteLine(queueUrl);
            Debug.WriteLine(queueUrl);
            Console.WriteLine(queueUrl);

            return queueUrl;
        }

        public async Task Send(MessageBase @message)
        {
            try
            {
                await _amazonSqs.SendMessageAsync(new SendMessageRequest
                {
                    QueueUrl = GetQueueUrl(@message.GetType()),
                    MessageDeduplicationId = Guid.NewGuid().ToString(),
                    MessageGroupId = _options.GroupId,
                    MessageBody = JsonConvert.SerializeObject(@message),
                });
            }
            catch (Exception ex)
            {

                throw new Exception(GetQueueUrl(@message.GetType()), ex);
            }

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

        public void Dispose()
        {
            _amazonSqs?.Dispose();
        }
    }
}