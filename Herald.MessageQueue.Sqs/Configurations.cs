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
                throw new ArgumentNullException(nameof(services));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            services.Configure(options);
            var messageQueueOptions = new MessageQueueOptions();
            options?.Invoke(messageQueueOptions);

            services.TryAddSingleton(messageQueueOptions);

            services.TryAddSingleton<IMessageQueue, MessageQueueSqs>();

            var url = $"{messageQueueOptions.Host}:{messageQueueOptions.Port}/queue";

            var awsSqsOptions = new AWSOptions();

            if (string.IsNullOrWhiteSpace(url))
            {
                awsSqsOptions.Region = RegionEndpoint.GetBySystemName(messageQueueOptions.RegionEndpoint);
            }
            else
            {
                awsSqsOptions.Region = null;
                awsSqsOptions.DefaultClientConfig.AllowAutoRedirect = false;
                awsSqsOptions.DefaultClientConfig.EndpointDiscoveryEnabled = false;
                awsSqsOptions.DefaultClientConfig.UseHttp = true;
                awsSqsOptions.DefaultClientConfig.DisableHostPrefixInjection = true;
                awsSqsOptions.DefaultClientConfig.ServiceURL = new Uri(url).GetLeftPart(System.UriPartial.Authority);
            }

            awsSqsOptions.DefaultClientConfig.Validate();

            services.AddDefaultAWSOptions(awsSqsOptions);
            services.AddAWSService<IAmazonSQS>(awsSqsOptions);

            return new MessageQueueBuilder(services);
        }
    }
}
