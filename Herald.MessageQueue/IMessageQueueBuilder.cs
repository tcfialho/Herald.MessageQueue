using Microsoft.Extensions.DependencyInjection;

namespace Herald.MessageQueue
{
    public interface IMessageQueueBuilder
    {
        IServiceCollection Services { get; }
    }
}
