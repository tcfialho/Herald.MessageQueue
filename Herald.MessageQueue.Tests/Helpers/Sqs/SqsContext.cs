
using System;
using System.IO;

namespace Herald.MessageQueue.Tests.Helpers.Sqs
{
    public class SqsContext : IDisposable
    {
        private static DockerCompose _dockerCompose;

        public DockerCompose DockerCompose
        {
            get => _dockerCompose;
        }

        public SqsContext()
        {
            const string pathToYml = @"..\..\..\..\.docker\test-enviroment.yml";

            if (!File.Exists(pathToYml))
                throw new FileNotFoundException();

            if (_dockerCompose == null)
                _dockerCompose = new DockerCompose(pathToYml, "SQS-DONE!");
        }

        public void Dispose()
        {
            _dockerCompose?.Dispose();
        }
    }
}
