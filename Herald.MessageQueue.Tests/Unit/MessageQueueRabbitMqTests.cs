using Herald.MessageQueue.RabbitMq;
using Herald.MessageQueue.Tests.Helpers.RabbitMq;

using Microsoft.Extensions.Configuration;

using Moq;

using Newtonsoft.Json;

using RabbitMQ.Client;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace Herald.MessageQueue.Tests.Unit
{
    public class MessageQueueRabbitMqTests
    {
        [Fact]
        public async Task ShouldSend()
        {
            //Arrange
            var modelMock = new Mock<IModel>();
            var connectionMock = new Mock<IConnection>();
            var configurationMock = new Mock<IConfiguration>();

            var messageQueueOptions = new MessageQueueOptions();

            modelMock.Setup(x => x.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>()))
                         .Verifiable();
            configurationMock.SetupGet(x => x[It.IsAny<string>()]).Returns(string.Empty);
            connectionMock.Setup(x => x.CreateModel()).Returns(modelMock.Object);

            var queue = new MessageQueueRabbitMq(connectionMock.Object, messageQueueOptions, 
                            new MessageQueueInfo(messageQueueOptions, configurationMock.Object));

            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };

            //Act
            await queue.Send(msg);

            //Assert
            modelMock.VerifyAll();
        }

        [Fact]
        public async Task ShouldReceive()
        {
            //Arrange
            var modelMock = new Mock<IModel>();
            var connectionMock = new Mock<IConnection>();
            var configurationMock = new Mock<IConfiguration>();

            var messageQueueOptions = new MessageQueueOptions();
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };

            modelMock.Setup(x => x.BasicGet(It.IsAny<string>(), It.IsAny<bool>()))
                         .Returns(new BasicGetResult(0, false, "", "", 1, null, Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(msg))))
                         .Verifiable();
            configurationMock.SetupGet(x => x[It.IsAny<string>()]).Returns(string.Empty);
            connectionMock.Setup(x => x.CreateModel()).Returns(modelMock.Object);

            var queue = new MessageQueueRabbitMq(connectionMock.Object, messageQueueOptions, 
                new MessageQueueInfo(messageQueueOptions, configurationMock.Object));

            //Act
            var qtd = 0;
            await foreach (var message in queue.Receive<TestMessage>(1))
            {
                qtd++;
            }

            //Assert
            modelMock.VerifyAll();
        }

        [Fact]
        public async Task WhenReceiveShouldValidateMaxNumberOfMessages()
        {
            //Arrange
            const int maxNumberOfMessages = 0;
            var modelMock = new Mock<IModel>();
            var connectionMock = new Mock<IConnection>();
            var configurationMock = new Mock<IConfiguration>();

            var messageQueueOptions = new MessageQueueOptions();
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };

            modelMock.Setup(x => x.BasicGet(It.IsAny<string>(), It.IsAny<bool>()))
                         .Returns(new BasicGetResult(0, false, "", "", 1, null, Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(msg))))
                         .Verifiable();
            configurationMock.SetupGet(x => x[It.IsAny<string>()]).Returns(string.Empty);

            var queue = new MessageQueueRabbitMq(connectionMock.Object, messageQueueOptions, 
                new MessageQueueInfo(messageQueueOptions, configurationMock.Object));

            //Act
            async Task act() => await queue.Receive<TestMessage>(maxNumberOfMessages)
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
            var modelMock = new Mock<IModel>();
            var connectionMock = new Mock<IConnection>();
            var configurationMock = new Mock<IConfiguration>();

            var messageQueueOptions = new MessageQueueOptions();
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };

            modelMock.Setup(x => x.BasicGet(It.IsAny<string>(), It.IsAny<bool>()))
                         .Returns(new BasicGetResult(0, false, "", "", 1, null, Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(msg))))
                         .Verifiable();
            configurationMock.SetupGet(x => x[It.IsAny<string>()]).Returns(string.Empty);
            connectionMock.Setup(x => x.CreateModel()).Returns(modelMock.Object);

            var queue = new MessageQueueRabbitMq(connectionMock.Object, messageQueueOptions, 
                new MessageQueueInfo(messageQueueOptions, configurationMock.Object));

            //Act
            var qtd = 0;
            await foreach (var message in queue.Receive<TestMessage>(cancellationToken))
            {
                qtd++;
            }

            //Assert
            connectionMock.VerifyAll();
            Assert.True(qtd > 0);
        }

        [Fact]
        public async Task ShouldMarkAsReceived()
        {
            //Arrange
            var modelMock = new Mock<IModel>();
            var connectionMock = new Mock<IConnection>();
            var configurationMock = new Mock<IConfiguration>();

            var messageQueueOptions = new MessageQueueOptions();

            (ulong DeliveryTag, CancellationTokenSource CancellationTokenSource) queueData = GetQueueDataStub();

            var msg = new TestMessage() { Id = Guid.NewGuid().ToString(), QueueData = queueData };

            modelMock.Setup(x => x.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()))
                         .Verifiable();
            configurationMock.SetupGet(x => x[It.IsAny<string>()]).Returns(string.Empty);
            connectionMock.Setup(x => x.CreateModel()).Returns(modelMock.Object);

            var queue = new MessageQueueRabbitMq(connectionMock.Object, messageQueueOptions,
                new MessageQueueInfo(messageQueueOptions, configurationMock.Object));

            //Act
            await queue.Received(msg);

            //Assert
            connectionMock.VerifyAll();
        }

        private static (ulong DeliveryTag, CancellationTokenSource CancellationTokenSource) GetQueueDataStub()
        {
            (ulong DeliveryTag, CancellationTokenSource CancellationTokenSource) queueData;
            queueData.DeliveryTag = 1;
            queueData.CancellationTokenSource = new CancellationTokenSource();
            return queueData;
        }
    }
}
