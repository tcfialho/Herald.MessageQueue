using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System;
using Herald.MessageQueue.Tests.Helpers;
using Xunit;

namespace Herald.MessageQueue.Tests.Integrated
{
    public abstract class MessageQueueTestsBase
    {
        protected IThreadSafeBuilder threadSafeBuilder;

        protected MessageQueueTestsBase(IThreadSafeBuilder threadSafeBuilder)
        {
            this.threadSafeBuilder = threadSafeBuilder;
        }

        [Fact]
        public async void ShouldSend()
        {
            //Arrange
            var msg = new TestMessageA() { Id = Guid.NewGuid().ToString() };
            using var queue = threadSafeBuilder.Build();
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
            using var queue = threadSafeBuilder.Build();
            for (int i = 0; i <= maxNumberOfMessages; i++)
            {
                await queue.Send(msg);
            }

            //Act
            var qtd = 0;
            TestMessageB receivedMessage = null;
            await foreach (var message in queue.Receive<TestMessageB>(maxNumberOfMessages))
            {
                receivedMessage = message;
                qtd++;
                await queue.Received(message);
            }

            //Assert            
            Assert.NotNull(receivedMessage);
            Assert.Equal(msg.Id, receivedMessage.Id);
            Assert.Equal(msg.QueueData, msg.QueueData);
            Assert.True(qtd > 0);
        }

        [Fact]
        public async Task ShouldWaitUntilReceiveMessage()
        {
            //Arrange
            const int timeoutSeconds = 5;
            var msg = new TestMessageD() { Id = Guid.NewGuid().ToString() };
            using var queue = threadSafeBuilder.Build();
            var stopWatch = new Stopwatch();

            //Act
            stopWatch.Start();
            await foreach (var message in queue.Receive<TestMessageD>(TimeSpan.FromSeconds(timeoutSeconds)))
            {
                Assert.NotNull(message);
            }
            stopWatch.Stop();

            //Assert
            Assert.True(Math.Round(stopWatch.Elapsed.TotalSeconds) == timeoutSeconds, $"Expected = {timeoutSeconds} but elapsed : {stopWatch.Elapsed.TotalSeconds}");
        }

        [Fact]
        public async Task ShouldMarkAsReceived()
        {
            //Arrange
            const int delay = 5;
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(delay)).Token;
            var msg = new TestMessageC() { Id = Guid.NewGuid().ToString() };
            using var queue = threadSafeBuilder.Build();
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