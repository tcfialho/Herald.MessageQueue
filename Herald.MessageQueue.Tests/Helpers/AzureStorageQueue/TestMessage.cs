namespace Herald.MessageQueue.Tests.Helpers.AzureStorageQueue
{
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

}
