using System;
using System.Threading.Tasks;

using Xunit;


namespace Herald.MessageQueue.Tests
{
    public class MessageQueueKafkaTests : IDisposable
    {
        private readonly IMessageQueue _queue;

        public MessageQueueKafkaTests()
        {
            _queue = KafkaThreadSafeBuilder.Build();
        }

        [Fact]
        public async void ShouldSend()
        {
            //Arrange
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };
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
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };
            await _queue.Send(msg);

            //Act
            var qtd = 0;
            await foreach (var message in _queue.Receive<TestMessage>())
                qtd++;

            //Assert
            Assert.True(qtd > 0);
        }

        [Fact]
        public async Task ShouldMarkAsReceived()
        {
            //Arrange
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };
            await _queue.Send(msg);
            Task received = null;

            //Act
            await foreach (var message in _queue.Receive<TestMessage>())
                received = _queue.Received(message);

            await received;

            //Assert
            Assert.False(received.IsFaulted);
        }

        [Fact]
        public async Task ShouldNotReReadReceivedMessages()
        {
            //Arrange
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };
            await _queue.Send(msg);
            var qtd = 0;

            //Act
            await foreach (var message in _queue.Receive<TestMessage>())
                await _queue.Received(message);
            await foreach (var message in _queue.Receive<TestMessage>())
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
