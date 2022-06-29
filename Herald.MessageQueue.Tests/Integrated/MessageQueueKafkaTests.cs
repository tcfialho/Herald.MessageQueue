using Herald.MessageQueue.Tests.Helpers.Kafka;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Xunit;


namespace Herald.MessageQueue.Tests.Integrated
{
    public class MessageQueueKafkaTests : MessageQueueTestsBase
    {
        public MessageQueueKafkaTests() : base(new KafkaThreadSafeBuilder())
        {
        }
    }
}
