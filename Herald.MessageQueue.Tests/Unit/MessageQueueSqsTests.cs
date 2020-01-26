using Amazon.SQS;
using Amazon.SQS.Model;

using Herald.MessageQueue.Sqs;
using Herald.MessageQueue.Tests.Helpers.Sqs;

using Moq;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace Herald.MessageQueue.Tests.Unit
{
    public class MessageQueueSqsTests
    {
        [Fact]
        public async Task ShouldSend()
        {
            //Arrange
            var amazonSqsMock = new Mock<IAmazonSQS>();
            var messageQueueOptions = new MessageQueueOptions();
            amazonSqsMock.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), CancellationToken.None))
                         .ReturnsAsync(new SendMessageResponse())
                         .Verifiable();
            var queue = new MessageQueueSqs(amazonSqsMock.Object, messageQueueOptions);
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };

            //Act
            await queue.Send(msg);

            //Assert
            amazonSqsMock.VerifyAll();
        }

        [Fact]
        public async Task ShouldReceive()
        {
            //Arrange
            var amazonSqsMock = new Mock<IAmazonSQS>();
            var messageQueueOptions = new MessageQueueOptions();
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };
            amazonSqsMock.Setup(x => x.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), CancellationToken.None))
                         .ReturnsAsync(new ReceiveMessageResponse()
                         {
                             Messages = new List<Message>()
                             {
                                 new Message()
                                 {
                                     Body = JsonConvert.SerializeObject(msg),
                                     ReceiptHandle = Guid.NewGuid().ToString()
                                 }
                             }
                         })
                         .Verifiable();
            var queue = new MessageQueueSqs(amazonSqsMock.Object, messageQueueOptions);


            //Act
            var qtd = 0;
            await foreach (var message in queue.Receive<TestMessage>(1))
                qtd++;

            //Assert
            amazonSqsMock.VerifyAll();
            Assert.True(qtd > 0);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(11)]
        public async Task WhenReceiveShouldValidateMaxNumberOfMessages(int maxNumberOfMessages)
        {
            //Arrange
            var amazonSqsMock = new Mock<IAmazonSQS>();
            var messageQueueOptions = new MessageQueueOptions();
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };
            amazonSqsMock.Setup(x => x.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), CancellationToken.None))
                         .ReturnsAsync(new ReceiveMessageResponse()
                         {
                             Messages = new List<Message>()
                             {
                                 new Message()
                                 {
                                     Body = JsonConvert.SerializeObject(msg),
                                     ReceiptHandle = Guid.NewGuid().ToString()
                                 }
                             }
                         })
                         .Verifiable();
            var queue = new MessageQueueSqs(amazonSqsMock.Object, messageQueueOptions);

            //Act
            Func<Task> act = async () => await queue.Receive<TestMessage>(maxNumberOfMessages)
                                                    .GetAsyncEnumerator()
                                                    .MoveNextAsync();

            //Assert
            await Assert.ThrowsAsync<ArgumentException>(act);
        }

        [Fact]
        public async Task ShouldReceiveWithCancellationToken()
        {
            //Arrange
            const int delay = 1;
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(delay)).Token;
            var amazonSqsMock = new Mock<IAmazonSQS>();
            var messageQueueOptions = new MessageQueueOptions();
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };
            amazonSqsMock.Setup(x => x.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), cancellationToken))
                         .ReturnsAsync(new ReceiveMessageResponse()
                         {
                             Messages = new List<Message>()
                             {
                                 new Message()
                                 {
                                     Body = JsonConvert.SerializeObject(msg),
                                     ReceiptHandle = Guid.NewGuid().ToString()
                                 }
                             }
                         })
                         .Verifiable();
            var queue = new MessageQueueSqs(amazonSqsMock.Object, messageQueueOptions);

            //Act
            var qtd = 0;
            await foreach (var message in queue.Receive<TestMessage>(cancellationToken))
                qtd++;

            //Assert
            amazonSqsMock.VerifyAll();
            Assert.True(qtd > 0);
        }

        [Fact]
        public async Task ShouldMarkAsReceived()
        {
            //Arrange
            var amazonSqsMock = new Mock<IAmazonSQS>();
            var messageQueueOptions = new MessageQueueOptions();
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };
            amazonSqsMock.Setup(x => x.DeleteMessageAsync(It.IsAny<DeleteMessageRequest>(), CancellationToken.None))
                         .ReturnsAsync(new DeleteMessageResponse()
                         {
                             HttpStatusCode = HttpStatusCode.OK
                         })
                         .Verifiable();
            var queue = new MessageQueueSqs(amazonSqsMock.Object, messageQueueOptions);

            //Act
            await queue.Received(msg);

            //Assert
            amazonSqsMock.VerifyAll();
        }
    }
}
