using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SQS;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System;

namespace Herald.MessageQueue.Sqs
{
    public static class Configurations
    {
        public static IMessageQueueBuilder AddMessageQueueSqs(this IServiceCollection services, Action<MessageQueueOptions> options)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            services.Configure(options);
            var messageQueueOptions = new MessageQueueOptions();
            options?.Invoke(messageQueueOptions);

            services.TryAddSingleton(messageQueueOptions);

            services.TryAddSingleton<IMessageQueue, MessageQueueSqs>();

            services.TryAddSingleton<IMessageQueueInfo, MessageQueueInfo>();

            var awsSqsOptions = new AWSOptions();

            if (string.IsNullOrWhiteSpace(messageQueueOptions.ServiceURL))
            {
                awsSqsOptions.Region = RegionEndpoint.GetBySystemName(messageQueueOptions.Region);
            }
            else
            {
                awsSqsOptions.Region = null;
                awsSqsOptions.DefaultClientConfig.AllowAutoRedirect = false;
                awsSqsOptions.DefaultClientConfig.EndpointDiscoveryEnabled = false;
                awsSqsOptions.DefaultClientConfig.UseHttp = true;
                awsSqsOptions.DefaultClientConfig.DisableHostPrefixInjection = true;
                awsSqsOptions.DefaultClientConfig.ServiceURL = messageQueueOptions.ServiceURL;
            }

            awsSqsOptions.DefaultClientConfig.Validate();

            services.AddDefaultAWSOptions(awsSqsOptions);
            services.AddAWSService<IAmazonSQS>(awsSqsOptions);

            return new MessageQueueBuilder(services);
        }
    }
}
