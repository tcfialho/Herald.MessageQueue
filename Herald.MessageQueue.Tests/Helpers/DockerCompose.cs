
using System;
using System.Diagnostics;
using System.Linq;

namespace Herald.MessageQueue.Tests
{
    public sealed class DockerCompose : IDisposable
    {
        private readonly string _pathToYml;

        public DockerCompose(string pathToYml, params string[] waitForString)
        {
            _pathToYml = cleanArguments(pathToYml);
            Down(_pathToYml);
            Up(_pathToYml, waitForString);
        }

        public void Dispose()
        {
            Down(_pathToYml);
        }

        public static void Up(string pathToYml, string[] waitForString = null)
        {
            RunProcess("docker-compose", $"-f \"{cleanArguments(pathToYml)}\" up", waitForString);
        }

        public static void Down(string pathToYml)
        {
            RunProcess("docker-compose", $"-f \"{cleanArguments(pathToYml)}\" down");
        }

        private static void RunProcess(string fileName, string arguments, string[] waitForStrings = null)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            if (waitForStrings != null && waitForStrings.Any())
            {
                var found = 0;
                while (!process.StandardOutput.EndOfStream)
                {
                    var line = process.StandardOutput.ReadLine();

                    if (waitForStrings.Any(stringToWait => line.Contains(stringToWait)))
                    {
                        found++;
                    }

                    if (waitForStrings.Length == found)
                    {
                        break;
                    }
                }
            }
            else
            {
                process.WaitForExit();
            }
        }

        private static string cleanArguments(string pathToYml)
        {
            return pathToYml.Replace("\"", "\\\"");
        }
    }
}
