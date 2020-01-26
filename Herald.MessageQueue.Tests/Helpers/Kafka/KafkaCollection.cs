
using Xunit;

namespace Herald.MessageQueue.Tests.Helpers.Kafka
{
    [CollectionDefinition("KafkaContext", DisableParallelization = true)]
    public class KafkaCollection : ICollectionFixture<KafkaContext>
    {
    }
}
