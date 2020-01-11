
using Xunit;

namespace Herald.MessageQueue.Tests
{
    [CollectionDefinition("KafkaContext", DisableParallelization = true)]
    public class KafkaCollection : ICollectionFixture<KafkaContext>
    {
    }
}
