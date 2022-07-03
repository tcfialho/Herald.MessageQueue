using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Herald.MessageQueue.AzureStorageQueue;
using Herald.MessageQueue.Tests.Helpers;

using Moq;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace Herald.MessageQueue.Tests.Unit
{
    public class MessageQueueAzureStoreQueueTests
    {
        [Fact]
        public async Task ShouldSend()
        {
            //Arrange
            var queueClientMock = new Mock<QueueClient>();
            queueClientMock
                .Setup(x => x.SendMessageAsync(It.IsAny<string>()))
                .Verifiable();

            var queueClientFactoryMock = new Mock<IQueueClientFactory>();
            queueClientFactoryMock
                .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(queueClientMock.Object)
                .Verifiable();

            var messageQueueOptions = new MessageQueueOptions();
            
            var messageQueueInfoMock = new Mock<IMessageQueueInfo>();
            messageQueueInfoMock
                .Setup(x => x.GetQueueName(It.IsAny<Type>()))
                .Verifiable();

            var queue = new MessageQueueAzureStorageQueue(queueClientFactoryMock.Object, messageQueueOptions, messageQueueInfoMock.Object);

            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };

            //Act
            await queue.Send(msg);

            //Assert
            queueClientMock.VerifyAll();
            queueClientFactoryMock.VerifyAll();
            messageQueueInfoMock.VerifyAll();
        }

        [Fact]
        public async Task ShouldReceive()
        {
            //Arrange
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };

            var queueMessage = QueuesModelFactory.QueueMessage(messageId: default, popReceipt: default, messageText: JsonConvert.SerializeObject(msg), dequeueCount: default);

            var mockMessageReponse = new Mock<Response<QueueMessage[]>>();
            mockMessageReponse
                .SetupGet(m => m.Value).Returns(new QueueMessage[] { queueMessage })
                .Verifiable();

            var queueClientMock = new Mock<QueueClient>();
            queueClientMock
                .Setup(x => x.ReceiveMessagesAsync(It.IsAny<int>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockMessageReponse.Object)
                .Verifiable();

            var queueClientFactoryMock = new Mock<IQueueClientFactory>();
            queueClientFactoryMock
                .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(queueClientMock.Object)
                .Verifiable();

            var messageQueueOptions = new MessageQueueOptions();

            var messageQueueInfoMock = new Mock<IMessageQueueInfo>();
            messageQueueInfoMock.Setup(x => x.GetQueueName(It.IsAny<Type>()))
                                 .Verifiable();

            var queue = new MessageQueueAzureStorageQueue(queueClientFactoryMock.Object, messageQueueOptions, messageQueueInfoMock.Object);

            //Act
            var qtd = 0;
            await foreach (var message in queue.Receive<TestMessage>(1))
            {
                qtd++;
            }

            //Assert
            mockMessageReponse.VerifyAll();
            queueClientMock.VerifyAll();
            queueClientFactoryMock.VerifyAll();
            messageQueueInfoMock.VerifyAll();
        }

        [Fact]
        public async Task WhenReceiveShouldValidateMaxNumberOfMessages()
        {
            //Arrange            
            var queueClientFactoryMock = new Mock<IQueueClientFactory>();
            var messageQueueOptions = new MessageQueueOptions();
            var messageQueueInfoMock = new Mock<IMessageQueueInfo>();

            var queue = new MessageQueueAzureStorageQueue(queueClientFactoryMock.Object, messageQueueOptions, messageQueueInfoMock.Object);

            var maxNumberOfMessages = 0;

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

            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };

            var queueMessage = QueuesModelFactory.QueueMessage(messageId: default, popReceipt: default, messageText: JsonConvert.SerializeObject(msg), dequeueCount: default);

            var mockMessageReponse = new Mock<Response<QueueMessage[]>>();
            mockMessageReponse
                .SetupGet(m => m.Value).Returns(new QueueMessage[] { queueMessage })
                .Verifiable();

            var queueClientMock = new Mock<QueueClient>();
            queueClientMock
                .Setup(x => x.ReceiveMessagesAsync(It.IsAny<int>(), It.IsAny<TimeSpan?>(), cancellationToken))
                .ReturnsAsync(mockMessageReponse.Object)
                .Verifiable();

            var queueClientFactoryMock = new Mock<IQueueClientFactory>();
            queueClientFactoryMock
                .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(queueClientMock.Object)
                .Verifiable();

            var messageQueueOptions = new MessageQueueOptions();

            var messageQueueInfoMock = new Mock<IMessageQueueInfo>();
            messageQueueInfoMock.Setup(x => x.GetQueueName(It.IsAny<Type>()))
                                 .Verifiable();

            var queue = new MessageQueueAzureStorageQueue(queueClientFactoryMock.Object, messageQueueOptions, messageQueueInfoMock.Object);
            //Act
            var qtd = 0;
            await foreach (var message in queue.Receive<TestMessage>(cancellationToken))
            {
                qtd++;
            }

            //Assert
            mockMessageReponse.VerifyAll();
            queueClientMock.VerifyAll();
            queueClientFactoryMock.VerifyAll();
            messageQueueInfoMock.VerifyAll();
            Assert.True(qtd > 0);
        }

        [Fact]
        public async Task ShouldMarkAsReceived()
        {
            //Arrange
            (string MessageId, string PopReceipt) queueData;
            queueData.MessageId = "";
            queueData.PopReceipt = "";

            var msg = new TestMessage() { Id = Guid.NewGuid().ToString(), QueueData = queueData };

            var queueMessage = QueuesModelFactory.QueueMessage(messageId: default, popReceipt: default, messageText: JsonConvert.SerializeObject(msg), dequeueCount: default);

            var queueClientMock = new Mock<QueueClient>();
            queueClientMock
                .Setup(x => x.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            var queueClientFactoryMock = new Mock<IQueueClientFactory>();
            queueClientFactoryMock
                .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(queueClientMock.Object)
                .Verifiable();

            var messageQueueOptions = new MessageQueueOptions();

            var messageQueueInfoMock = new Mock<IMessageQueueInfo>();
            messageQueueInfoMock
                .Setup(x => x.GetQueueName(It.IsAny<Type>()))
                .Verifiable();

            var queue = new MessageQueueAzureStorageQueue(queueClientFactoryMock.Object, messageQueueOptions, messageQueueInfoMock.Object);

            //Act
            await queue.Received(msg);

            //Assert
            queueClientMock.VerifyAll();
            queueClientFactoryMock.VerifyAll();
            messageQueueInfoMock.VerifyAll();
        }
    }
}
