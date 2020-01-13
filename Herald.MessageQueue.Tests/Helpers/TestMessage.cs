namespace Herald.MessageQueue.Tests
{
    public class TestMessage : MessageBase
    {
        public TestMessage() : base()
        {
        }
        public string Id { get; set; }
    }
}
