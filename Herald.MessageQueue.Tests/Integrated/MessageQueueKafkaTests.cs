using Herald.MessageQueue.Tests.Helpers.Kafka;


namespace Herald.MessageQueue.Tests.Integrated
{
    public class MessageQueueKafkaTests : MessageQueueTestsBase
    {
        public MessageQueueKafkaTests() : base(new KafkaThreadSafeBuilder())
        {
        }
    }
}
