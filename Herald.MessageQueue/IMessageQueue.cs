using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Herald.MessageQueue
{
    public interface IMessageQueue : IDisposable
    {
        Task Send(MessageBase @event);

        Task Received(MessageBase @event);

        IAsyncEnumerable<TMessage> Receive<TMessage>() where TMessage : MessageBase;

        IAsyncEnumerable<TMessage> Receive<TMessage>(CancellationToken cancellationToken) where TMessage : MessageBase;
    }
}
