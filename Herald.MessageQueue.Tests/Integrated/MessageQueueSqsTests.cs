using Herald.MessageQueue.Tests.Helpers.Sqs;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace Herald.MessageQueue.Tests.Integrated
{
    public class MessageQueueSqsTests : MessageQueueTestsBase
    {
        public MessageQueueSqsTests() : base(new SqsThreadSafeBuilder())
        {
        }
    }
}
