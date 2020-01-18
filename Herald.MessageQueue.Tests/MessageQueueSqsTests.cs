﻿using System;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace Herald.MessageQueue.Tests
{
    public class MessageQueueSqsTests : IDisposable
    {
        private readonly IMessageQueue _queue;

        public MessageQueueSqsTests()
        {
            _queue = SqsThreadSafeBuilder.Build();
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
            const int maxNumberOfMessages = 5;
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };
            await _queue.Send(msg);

            //Act
            var qtd = 0;
            await foreach (var message in _queue.Receive<TestMessage>(maxNumberOfMessages))
                qtd++;

            //Assert
            Assert.True(qtd > 0);
        }

        [Fact]
        public async Task ShouldReceiveUntilCanceled()
        {
            //Arrange
            const int delay = 8;
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(delay)).Token;
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };
            await _queue.Send(msg);

            //Act
            var qtd = 0;
            await foreach (var message in _queue.Receive<TestMessage>(cancellationToken))
                qtd++;

            //Assert
            Assert.True(qtd > 0);
        }

        [Fact]
        public async Task ShouldMarkAsReceived()
        {
            //Arrange
            const int delay = 5;
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(delay)).Token;
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };
            await _queue.Send(msg);
            Task received = null;

            //Act
            await foreach (var message in _queue.Receive<TestMessage>(cancellationToken))
                received = _queue.Received(message);

            await received;

            //Assert
            Assert.False(received.IsFaulted);
        }

        [Fact]
        public async Task ShouldNotReReadReceivedMessages()
        {
            //Arrange
            const int delay = 5;
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(delay)).Token;
            var msg = new TestMessage() { Id = Guid.NewGuid().ToString() };
            await _queue.Send(msg);
            var qtd = 0;

            //Act
            await foreach (var message in _queue.Receive<TestMessage>(cancellationToken))
                await _queue.Received(message);
            await foreach (var message in _queue.Receive<TestMessage>(cancellationToken))
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
