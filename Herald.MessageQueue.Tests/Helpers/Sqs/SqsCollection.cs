
using Xunit;

namespace Herald.MessageQueue.Tests
{
    [CollectionDefinition("SqsContext", DisableParallelization = true)]
    public class SqsCollection : ICollectionFixture<SqsContext>
    {
    }
}
