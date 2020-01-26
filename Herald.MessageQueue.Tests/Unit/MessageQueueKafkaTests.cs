using Confluent.Kafka;

using Herald.MessageQueue.Kafka;
using Herald.MessageQueue.Tests.Helpers.Kafka;

using Moq;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Xunit;


namespace Herald.MessageQueue.Tests.Unit
{
    public class MessageQueueKafkaTests
    {
        [Fact]
        public async Task ShouldSend()
        {
            //Arrange
            var consumerMock = new Mock<IConsumer<Ignore, string>>();
            var producerMock = new Mock<IProducer<Null, string>>();
            var messageQueueOptions = new MessageQueueOptions();
            producerMock.Setup(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<Null, string>>()))
                        .Verifiable();
            var queue = new MessageQueueKafka(consumerMock.Object, producerMock.Object, messageQueueOptions);
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };

            //Act
            await queue.Send(msg);

            //Assert
            producerMock.VerifyAll();
        }

        [Fact]
        public async Task ShouldReceive()
        {
            //Arrange
            var consumerMock = new Mock<IConsumer<Ignore, string>>();
            var producerMock = new Mock<IProducer<Null, string>>();
            var consumeResultMock = new Mock<ConsumeResult<Ignore, string>>();
            var messageQueueOptions = new MessageQueueOptions();
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };
            consumerMock.SetupGet(x => x.Subscription).Returns(() => new List<string>()).Verifiable();
            consumerMock.Setup(x => x.Subscribe(It.IsAny<string>())).Verifiable();
            consumerMock.Setup(x => x.Consume(TimeSpan.FromSeconds(5)))
                        .Returns(new ConsumeResult<Ignore, string>()
                        {
                            Message = new Message<Ignore, string>()
                            {
                                Value = JsonConvert.SerializeObject(msg)
                            }
                        })
                        .Verifiable();
            var queue = new MessageQueueKafka(consumerMock.Object, producerMock.Object, messageQueueOptions);

            //Act
            var qtd = 0;
            await foreach (var message in queue.Receive<TestMessage>(1))
                qtd++;

            //Assert
            consumerMock.VerifyAll();
        }

        [Fact]
        public async Task WhenReceiveShouldValidateMaxNumberOfMessages()
        {
            //Arrange
            const int maxNumberOfMessages = 0;
            var consumerMock = new Mock<IConsumer<Ignore, string>>();
            var producerMock = new Mock<IProducer<Null, string>>();
            var consumeResultMock = new Mock<ConsumeResult<Ignore, string>>();
            var messageQueueOptions = new MessageQueueOptions();
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };
            consumerMock.SetupGet(x => x.Subscription).Returns(() => new List<string>()).Verifiable();
            consumerMock.Setup(x => x.Subscribe(It.IsAny<string>())).Verifiable();
            consumerMock.Setup(x => x.Consume(TimeSpan.FromSeconds(5)))
                        .Returns(new ConsumeResult<Ignore, string>()
                        {
                            Message = new Message<Ignore, string>()
                            {
                                Value = JsonConvert.SerializeObject(msg)
                            }
                        })
                        .Verifiable();
            var queue = new MessageQueueKafka(consumerMock.Object, producerMock.Object, messageQueueOptions);

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
            var consumerMock = new Mock<IConsumer<Ignore, string>>();
            var producerMock = new Mock<IProducer<Null, string>>();
            var consumeResultMock = new Mock<ConsumeResult<Ignore, string>>();
            var messageQueueOptions = new MessageQueueOptions();
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };
            consumerMock.SetupGet(x => x.Subscription).Returns(() => new List<string>()).Verifiable();
            consumerMock.Setup(x => x.Subscribe(It.IsAny<string>())).Verifiable();
            consumerMock.Setup(x => x.Consume(cancellationToken))
                        .Returns(new ConsumeResult<Ignore, string>()
                        {
                            Message = new Message<Ignore, string>()
                            {
                                Value = JsonConvert.SerializeObject(msg)
                            }
                        })
                        .Verifiable();
            var queue = new MessageQueueKafka(consumerMock.Object, producerMock.Object, messageQueueOptions);

            //Act
            var qtd = 0;
            await foreach (var message in queue.Receive<TestMessage>(cancellationToken))
                qtd++;

            //Assert
            consumerMock.VerifyAll();
            Assert.True(qtd > 0);
        }

        [Fact]
        public async Task ShouldMarkAsReceived()
        {
            //Arrange
            var consumerMock = new Mock<IConsumer<Ignore, string>>();
            var producerMock = new Mock<IProducer<Null, string>>();
            var consumeResultMock = new Mock<ConsumeResult<Ignore, string>>();
            var messageQueueOptions = new MessageQueueOptions();
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };
            consumerMock.Setup(x => x.StoreOffset(It.IsAny<ConsumeResult<Ignore, string>>()))
                        .Verifiable();
            var queue = new MessageQueueKafka(consumerMock.Object, producerMock.Object, messageQueueOptions);

            //Act
            await queue.Received(msg);

            //Assert
            consumerMock.VerifyAll();
        }
    }
}
