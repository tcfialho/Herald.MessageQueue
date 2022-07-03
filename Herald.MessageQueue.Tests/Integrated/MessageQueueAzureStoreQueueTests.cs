using Herald.MessageQueue.Tests.Helpers.AzureStorageQueue;

namespace Herald.MessageQueue.Tests.Integrated
{
    public class MessageQueueAzureStoreQueueTests : MessageQueueTestsBase
    {
        public MessageQueueAzureStoreQueueTests() : base(new AzureStorageQueueThreadSafeBuilder())
        {
        }
    }
}
