﻿using Amazon;
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

            var awsSqsOptions = CreateAwsOptions(messageQueueOptions.RegionEndpoint, url);

            services.AddDefaultAWSOptions(awsSqsOptions);
            services.AddAWSService<IAmazonSQS>(awsSqsOptions);

            return new MessageQueueBuilder(services);
        }

        private static AWSOptions CreateAwsOptions(string regionEndpoint, string url)
        {
            var awsOptions = new AWSOptions();

            if (string.IsNullOrWhiteSpace(url))
            {
                awsOptions.Region = RegionEndpoint.GetBySystemName(regionEndpoint);
            }
            else
            {
                awsOptions.Region = null;
                awsOptions.DefaultClientConfig.AllowAutoRedirect = false;
                awsOptions.DefaultClientConfig.EndpointDiscoveryEnabled = false;
                awsOptions.DefaultClientConfig.UseHttp = true;
                awsOptions.DefaultClientConfig.DisableHostPrefixInjection = true;
                awsOptions.DefaultClientConfig.ServiceURL = new Uri(url).GetLeftPart(System.UriPartial.Authority);
            }

            awsOptions.DefaultClientConfig.Validate();

            return awsOptions;
        }
    }
}
