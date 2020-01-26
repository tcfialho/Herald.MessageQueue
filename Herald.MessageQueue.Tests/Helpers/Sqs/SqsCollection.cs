
using Xunit;

namespace Herald.MessageQueue.Tests.Helpers.Sqs
{
    [CollectionDefinition("SqsContext", DisableParallelization = true)]
    public class SqsCollection : ICollectionFixture<SqsContext>
    {
    }
}
