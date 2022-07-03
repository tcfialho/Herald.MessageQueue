using Herald.MessageQueue.Tests.Helpers.Sqs;

namespace Herald.MessageQueue.Tests.Integrated
{
    public class MessageQueueSqsTests : MessageQueueTestsBase
    {
        public MessageQueueSqsTests() : base(new SqsThreadSafeBuilder())
        {
        }
    }
}
