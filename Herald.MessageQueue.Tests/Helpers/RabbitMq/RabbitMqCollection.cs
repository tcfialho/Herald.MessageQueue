
using Xunit;

namespace Herald.MessageQueue.Tests.Helpers.RabbitMq
{
    [CollectionDefinition("RabbitMqContext", DisableParallelization = true)]
    public class RabbitMqCollection : ICollectionFixture<RabbitMqContext>
    {
    }
}
