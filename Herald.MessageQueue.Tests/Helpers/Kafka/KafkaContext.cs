
using System;
using System.IO;

namespace Herald.MessageQueue.Tests.Helpers.Kafka
{
    public class KafkaContext : IDisposable
    {
        private static DockerCompose _dockerCompose;

        public DockerCompose DockerCompose
        {
            get
            {
                return _dockerCompose;
            }
        }

        public KafkaContext()
        {
            const string pathToYml = @"..\..\..\..\.docker\test-enviroment.yml";

            if (!File.Exists(pathToYml))
            {
                throw new FileNotFoundException();
            }

            if (_dockerCompose == null)
            {
                _dockerCompose = new DockerCompose(pathToYml, "KAFKA-DONE!");
            }
        }

        public void Dispose()
        {
            _dockerCompose?.Dispose();
        }
    }
}
