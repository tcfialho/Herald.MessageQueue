using Microsoft.Extensions.DependencyInjection;

using System;

namespace Herald.MessageQueue
{
    public class MessageQueueBuilder : IMessageQueueBuilder
    {
        public IServiceCollection Services { get; }

        public MessageQueueBuilder(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }

            Services = services;
        }
    }
}
