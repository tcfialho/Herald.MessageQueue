
using Xunit;

namespace Herald.MessageQueue.Tests
{
    [CollectionDefinition("RabbitMqContext", DisableParallelization = true)]
    public class RabbitMqCollection : ICollectionFixture<RabbitMqContext>
    {
    }
}
