using Herald.MessageQueue.Tests.Helpers.RabbitMq;

namespace Herald.MessageQueue.Tests.Integrated
{
    public class MessageQueueRabbitMqTests : MessageQueueTestsBase
    {
        public MessageQueueRabbitMqTests() : base(new RabbitMqThreadSafeBuilder())
        {
        }
    }
}
