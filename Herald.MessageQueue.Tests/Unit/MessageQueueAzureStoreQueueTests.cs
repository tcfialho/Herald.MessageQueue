using Herald.MessageQueue.AzureStorageQueue;
using Herald.MessageQueue.Tests.Helpers.RabbitMq;

using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Queue;

using Moq;

using Newtonsoft.Json;

using System;
using System.Net.Http;
using System.Text;
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
            var clouldQueueClientMock = new Mock<CloudQueueClient>(MockBehavior.Loose, new Uri("http://localhost"), (StorageCredentials)null, (DelegatingHandler)null);
            var clouldQueue = new Mock<CloudQueue>(MockBehavior.Loose, new Uri("http://localhost"), (StorageCredentials)null);
            var messageQueueOptions = new MessageQueueOptions();

            clouldQueue.Setup(x => x.AddMessageAsync(It.IsAny<CloudQueueMessage>()))
                       .Returns(Task.FromResult(new CloudQueueMessage(new byte[0])))
                       .Verifiable();

            clouldQueueClientMock.Setup(x => x.GetQueueReference(It.IsAny<string>()))
                                 .Returns(clouldQueue.Object)
                                 .Verifiable();

            var queue = new MessageQueueAzureStorageQueue(clouldQueueClientMock.Object, messageQueueOptions);
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };

            //Act
            await queue.Send(msg);

            //Assert
            clouldQueueClientMock.VerifyAll();
            clouldQueue.VerifyAll();
        }

        [Fact]
        public async Task ShouldReceive()
        {
            //Arrange
            var clouldQueueClientMock = new Mock<CloudQueueClient>(MockBehavior.Loose, new Uri("http://localhost"), (StorageCredentials)null, (DelegatingHandler)null);
            var clouldQueue = new Mock<CloudQueue>(MockBehavior.Loose, new Uri("http://localhost"), (StorageCredentials)null);
            var messageQueueOptions = new MessageQueueOptions();

            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };

            clouldQueue.Setup(x => x.GetMessageAsync())
                         .ReturnsAsync(new CloudQueueMessage(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(msg))))
                         .Verifiable();

            clouldQueueClientMock.Setup(x => x.GetQueueReference(It.IsAny<string>()))
                                 .Returns(clouldQueue.Object)
                                 .Verifiable();

            var queue = new MessageQueueAzureStorageQueue(clouldQueueClientMock.Object, messageQueueOptions);

            //Act
            var qtd = 0;
            await foreach (var message in queue.Receive<TestMessage>(1))
                qtd++;

            //Assert
            clouldQueueClientMock.VerifyAll();
            clouldQueue.VerifyAll();
        }

        [Fact]
        public async Task WhenReceiveShouldValidateMaxNumberOfMessages()
        {
            //Arrange
            const int maxNumberOfMessages = 0;
            var clouldQueueClientMock = new Mock<CloudQueueClient>(MockBehavior.Loose, new Uri("http://localhost"), (StorageCredentials)null, (DelegatingHandler)null);
            var clouldQueue = new Mock<CloudQueue>(MockBehavior.Loose, new Uri("http://localhost"), (StorageCredentials)null);
            var messageQueueOptions = new MessageQueueOptions();

            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };

            clouldQueue.Setup(x => x.GetMessageAsync())
                         .ReturnsAsync(new CloudQueueMessage(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(msg))))
                         .Verifiable();

            clouldQueueClientMock.Setup(x => x.GetQueueReference(It.IsAny<string>()))
                                 .Returns(clouldQueue.Object)
                                 .Verifiable();

            var queue = new MessageQueueAzureStorageQueue(clouldQueueClientMock.Object, messageQueueOptions);

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
            var clouldQueueClientMock = new Mock<CloudQueueClient>(MockBehavior.Loose, new Uri("http://localhost"), (StorageCredentials)null, (DelegatingHandler)null);
            var clouldQueue = new Mock<CloudQueue>(MockBehavior.Loose, new Uri("http://localhost"), (StorageCredentials)null);
            var messageQueueOptions = new MessageQueueOptions();

            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };

            clouldQueue.Setup(x => x.GetMessageAsync(cancellationToken))
                         .ReturnsAsync(new CloudQueueMessage(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(msg))))
                         .Verifiable();

            clouldQueueClientMock.Setup(x => x.GetQueueReference(It.IsAny<string>()))
                                 .Returns(clouldQueue.Object)
                                 .Verifiable();

            var queue = new MessageQueueAzureStorageQueue(clouldQueueClientMock.Object, messageQueueOptions);

            //Act
            var qtd = 0;
            await foreach (var message in queue.Receive<TestMessage>(cancellationToken))
                qtd++;

            //Assert
            clouldQueueClientMock.VerifyAll();
            clouldQueue.VerifyAll();
            Assert.True(qtd > 0);
        }

        [Fact]
        public async Task ShouldMarkAsReceived()
        {
            //Arrange
            var clouldQueueClientMock = new Mock<CloudQueueClient>(MockBehavior.Loose, new Uri("http://localhost"), (StorageCredentials)null, (DelegatingHandler)null);
            var clouldQueue = new Mock<CloudQueue>(MockBehavior.Loose, new Uri("http://localhost"), (StorageCredentials)null);
            var messageQueueOptions = new MessageQueueOptions();

            var msg = new TestMessage() { Id = Guid.NewGuid().ToString(), QueueData = (ulong)0 };

            clouldQueue.Setup(x => x.DeleteMessageAsync(It.IsAny<CloudQueueMessage>())).Verifiable();

            clouldQueueClientMock.Setup(x => x.GetQueueReference(It.IsAny<string>()))
                                 .Returns(clouldQueue.Object)
                                 .Verifiable();

            var queue = new MessageQueueAzureStorageQueue(clouldQueueClientMock.Object, messageQueueOptions);

            //Act
            await queue.Received(msg);

            //Assert
            clouldQueueClientMock.VerifyAll();
            clouldQueue.VerifyAll();
        }
    }
}
