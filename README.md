# Herald.MessageQueue

![Status](https://github.com/tcfialho/Herald.MessageQueue/workflows/Herald.MessageQueue/badge.svg) ![Coverage](https://codecov.io/gh/tcfialho/Herald.MessageQueue/branch/master/graph/badge.svg) ![NuGet](https://buildstats.info/nuget/Herald.MessageQueue)

## Overview
Herald.MessageQueue is a set of building blocks for working with different types of message brokers using a simple "foreach" abstraction.

## Installation

Herald.MessageQueue have different packages, each handle a 3rd library message queue.

- Kafka
    - Package Manager

        ```
        Install-Package Herald.MessageQueue.Kafka
        ```
    - .NET CLI
        ```
        dotnet add package Herald.MessageQueue.Kafka
        ```

- RabbitMq
    - Package Manager

        ```
        Install-Package Herald.MessageQueue.RabbitMq
        ```
    - .NET CLI
        ```
        dotnet add package Herald.MessageQueue.RabbitMq
        ```

- Amazon Sqs
    - Package Manager

        ```
        Install-Package Herald.MessageQueue.Sqs
        ```
    - .NET CLI
        ```
        dotnet add package Herald.MessageQueue.Sqs
        ```

- Azure Storage Queue
    - Package Manager

        ```
        Install-Package Herald.MessageQueue.AzureStorageQueue
        ```
    - .NET CLI
        ```
        dotnet add package Herald.MessageQueue.AzureStorageQueue
        ```

## Samples
- [Herald.MessageQueue.Samples](https://github.com/tcfialho/Herald.MessageQueue.Samples)

## HealthCheck
- [Herald.MessageQueue.HealthCheck](https://github.com/tcfialho/Herald.MessageQueue.HealthCheck)

## Credits

Author [**Thiago Fialho**](https://br.linkedin.com/in/thiago-fialho-139ab116)

## License

Herald.MessageQueue is licensed under the [MIT License](LICENSE).