
using Xunit;

namespace Herald.MessageQueue.Tests.Helpers.AzureStorageQueue
{
    [CollectionDefinition("AzureStorageQueueContext", DisableParallelization = true)]
    public class AzureStorageQueueCollection : ICollectionFixture<AzureStorageQueueContext>
    {
    }
}
