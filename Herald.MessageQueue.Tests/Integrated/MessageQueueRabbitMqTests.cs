using Herald.MessageQueue.Tests.Helpers.RabbitMq;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace Herald.MessageQueue.Tests.Integrated
{
    public class MessageQueueRabbitMqTests : MessageQueueTestsBase
    {
        public MessageQueueRabbitMqTests() : base(new RabbitMqThreadSafeBuilder())
        {
        }
    }
}
