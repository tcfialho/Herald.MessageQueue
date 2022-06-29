namespace Herald.MessageQueue.Tests.Helpers
{
    public interface IThreadSafeBuilder
    {
        IMessageQueue Build();
    }
}