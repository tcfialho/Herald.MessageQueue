using Herald.MessageQueue.Attributes;

namespace Herald.MessageQueue.Tests.Helpers.RabbitMq
{
    [ExchangeName("TestMessage_Exchange")]
    [QueueName("TestMessage_Queue")]
    [RoutingKey("TestMessage_RoutingKey")]
    public class TestMessage : MessageBase
    {
        public TestMessage() : base()
        {
        }
        public string Id { get; set; }
    }

    public class TestMessageA : MessageBase
    {
        public TestMessageA() : base()
        {
        }
        public string Id { get; set; }
    }

    public class TestMessageB : MessageBase
    {
        public TestMessageB() : base()
        {
        }
        public string Id { get; set; }
    }

    public class TestMessageC : MessageBase
    {
        public TestMessageC() : base()
        {
        }
        public string Id { get; set; }
    }

    public class TestMessageD : MessageBase
    {
        public TestMessageD() : base()
        {
        }
        public string Id { get; set; }
    }
}
