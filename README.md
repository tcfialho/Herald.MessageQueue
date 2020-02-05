# Herald.MessageQueue

![Status](https://github.com/tcfialho/Herald.MessageQueue/workflows/Herald.MessageQueue/badge.svg) ![Coverage](https://codecov.io/gh/tcfialho/Herald.MessageQueue/branch/master/graph/badge.svg) ![NuGet](https://buildstats.info/nuget/Herald.MessageQueue)

Herald.MessageQueue is a set of building blocks for working with different types of message brokers using a simple queue abstraction.

# Installation Guide
- Install-Package Herald.MessageQueue.Kafka
- Install-Package Herald.MessageQueue.RabbitMq
- Install-Package Herald.MessageQueue.Sqs

# Samples
- [Herald.MessageQueue.Samples](https://github.com/tcfialho/Herald.MessageQueue.Samples)

- Dependecy Injection
```C#
namespace Worker
{
    public class Startup
    {
        //...
        public void ConfigureServices(IServiceCollection services)
        {
            //...
            services.AddMessageQueueRabbitMq(setup =>
            {
                setup.Host = "localhost";
                setup.Port = "5672";
                setup.Username = "myUserName";
                setup.Password = "myPassword";
            });
            //...
        }
        //...
    }
}
```

- Worker Consumer
```C#
namespace Worker
{
    public class Task : BackgroundService
    {
        private readonly IMessageQueue _queue;

        public Task(IMessageQueue queue)
        {
            _queue= queue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await foreach (var message in _queue.Receive<MyMessage>(stoppingToken))
                {
                    //YourCode
                }
            }
            await Task.CompletedTask;
        }
    }
}
```

# HealthCheck
- [Herald.MessageQueue.HealthCheck](https://github.com/tcfialho/Herald.MessageQueue.HealthCheck)

# Integration Testing
- The integration tests is designed to run with Docker. If your platform supports docker natively, you can simply install it and ensure that you have at least 4GiB of memory allocated to the docker engine.

