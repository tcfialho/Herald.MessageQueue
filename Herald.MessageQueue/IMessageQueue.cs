using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue
{
    public interface IMessageQueue : IDisposable
    {
        Task Send<TMessage>(TMessage message) where TMessage : MessageBase;

        Task Received<TMessage>(TMessage message) where TMessage : MessageBase;

        IAsyncEnumerable<TMessage> Receive<TMessage>(int maxNumberOfMessages) where TMessage : MessageBase;

        IAsyncEnumerable<TMessage> Receive<TMessage>(CancellationToken cancellationToken = default) where TMessage : MessageBase;

        async IAsyncEnumerable<TMessage> Receive<TMessage>(TimeSpan timeout) where TMessage : MessageBase
        {
            if (timeout == default)
            {
                throw new ArgumentException("Timeout of messages should be greater than zero.");
            }

            var cancellationTokenSource = new CancellationTokenSource(timeout);
            var cancellationToken = cancellationTokenSource.Token;

            await foreach (var message in Receive<TMessage>(cancellationToken))
            {
                yield return message;
            }
        }
    }
}
