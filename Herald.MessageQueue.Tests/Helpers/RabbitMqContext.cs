﻿
using System;
using System.IO;

namespace Herald.MessageQueue.Tests
{
    public class RabbitMqContext : IDisposable
    {
        private static DockerCompose _dockerCompose;

        public DockerCompose DockerCompose
        {
            get => _dockerCompose;
        }

        public RabbitMqContext()
        {
            const string pathToYml = @"..\..\..\..\.docker\test-enviroment.yml";

            if (!File.Exists(pathToYml))
                throw new FileNotFoundException();

            if (_dockerCompose == null)
                _dockerCompose = new DockerCompose(pathToYml, "RABBIT-DONE!");
        }

        public void Dispose()
        {
            _dockerCompose?.Dispose();
        }
    }
}
