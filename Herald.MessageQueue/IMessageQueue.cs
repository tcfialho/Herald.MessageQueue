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
            await foreach (var item in Receive<TMessage>(new CancellationTokenSource(timeout).Token))
            {
                yield return await Task.FromResult(item);
            }
        }
    }
}
