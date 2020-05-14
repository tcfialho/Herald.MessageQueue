using System;
using System.IO;

using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: Xunit.TestFramework("Herald.MessageQueue.Tests.TestEnviroment", "Herald.MessageQueue.Tests")]

namespace Herald.MessageQueue.Tests
{
    public class TestEnviroment : XunitTestFramework
    {
        private static DockerCompose _dockerCompose;

        public DockerCompose DockerCompose
        {
            get
            {
                return _dockerCompose;
            }
        }

        public TestEnviroment(IMessageSink messageSink)
          : base(messageSink)
        {
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SKIP")))
            {
                return;
            }

            const string pathToYml = @"..\..\..\..\.docker\test-enviroment.yml";

            if (!File.Exists(pathToYml))
            {
                throw new FileNotFoundException();
            }

            if (_dockerCompose == null)
            {
                _dockerCompose = new DockerCompose(pathToYml, "KAFKA-DONE!", "RABBIT-DONE!");
            }
        }

        public new void Dispose()
        {
            _dockerCompose?.Dispose();
            base.Dispose();
        }
    }
}