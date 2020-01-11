using System;
using System.Threading.Tasks;

using Xunit;

namespace Herald.MessageQueue.Tests
{
    public class MessageQueueSqsTests : IDisposable
    {
        private readonly IMessageQueue _queue;

        public MessageQueueSqsTests()
        {
            _queue = RabbitMqThreadSafeBuilder.Build();
        }

        [Fact]
        public async void ShouldSend()
        {
            //Arrange
            var msg = new Test() { Id = Guid.NewGuid().ToString() };
            Task send() => _queue.Send(msg);

            //Act
            await send();

            //Assert
            Assert.False(send().IsFaulted);
        }

        [Fact]
        public async Task ShouldReceive()
        {
            //Arrange
            var msg = new Test() { Id = Guid.NewGuid().ToString() };
            await _queue.Send(msg);

            //Act
            var qtd = 0;
            await foreach (var message in _queue.Receive<Test>())
                qtd++;

            //Assert
            Assert.True(qtd > 0);
        }

        [Fact]
        public async Task ShouldMarkAsReceived()
        {
            //Arrange
            var msg = new Test() { Id = Guid.NewGuid().ToString() };
            await _queue.Send(msg);

            //Act
            Task received = null;
            await foreach (var message in _queue.Receive<Test>())
                received = _queue.Received(message);
            await received;

            //Assert
            Assert.False(received.IsFaulted);
        }

        [Fact]
        public async Task ShouldNotReReadReceivedMessages()
        {
            //Arrange
            var msg = new Test() { Id = Guid.NewGuid().ToString() };
            await _queue.Send(msg);

            //Act
            var qtd = 0;
            await foreach (var message in _queue.Receive<Test>())
                await _queue.Received(message);
            await foreach (var message in _queue.Receive<Test>())
                qtd++;

            //Assert
            Assert.True(qtd == 0);
        }

        public void Dispose()
        {
            _queue.Dispose();
        }
    }
}
