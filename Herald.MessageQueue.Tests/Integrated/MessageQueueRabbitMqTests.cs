﻿using Herald.MessageQueue.Tests.Helpers.RabbitMq;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace Herald.MessageQueue.Tests.Integrated
{
    public class MessageQueueRabbitMqTests
    {
        [Fact]
        public async void ShouldSend()
        {
            //Arrange
            var msg = new TestMessageA() { Id = Guid.NewGuid().ToString() };
            using var queue = RabbitMqThreadSafeBuilder.Build(nameof(TestMessageA));
            Task send() => queue.Send(msg);

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
            var msg = new TestMessageB() { Id = Guid.NewGuid().ToString() };
            using var queue = RabbitMqThreadSafeBuilder.Build(nameof(TestMessageB));
            await queue.Send(msg);

            //Act
            var qtd = 0;
            await foreach (var message in queue.Receive<TestMessageB>(maxNumberOfMessages))
            {
                qtd++;
            }

            //Assert
            Assert.True(qtd > 0);
        }

        [Fact]
        public async Task ShouldWaitUntilReceiveMessage()
        {
            //Arrange
            const int timeoutSeconds = 5;
            var msg = new TestMessageB() { Id = Guid.NewGuid().ToString() };
            using var queue = RabbitMqThreadSafeBuilder.Build(nameof(TestMessageB));
            var stopWatch = new Stopwatch();

            //Act
            stopWatch.Start();
            await foreach (var message in queue.Receive<TestMessageB>(TimeSpan.FromSeconds(timeoutSeconds)))
            {
                Assert.NotNull(message);
            }
            stopWatch.Stop();

            //Assert
            Assert.True(stopWatch.Elapsed.TotalSeconds >= timeoutSeconds);
        }

        [Fact]
        public async Task ShouldMarkAsReceived()
        {
            //Arrange
            const int delay = 5;
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(delay)).Token;
            var msg = new TestMessageC() { Id = Guid.NewGuid().ToString() };
            using var queue = RabbitMqThreadSafeBuilder.Build(nameof(TestMessageC));
            await queue.Send(msg);
            var qtd = 0;

            //Act
            await foreach (var message in queue.Receive<TestMessageC>(cancellationToken))
            {
                await queue.Received(message);
            }

            await foreach (var message in queue.Receive<TestMessageC>(cancellationToken))
            {
                qtd++;
            }

            //Assert
            Assert.True(qtd == 0);
        }
    }
}
