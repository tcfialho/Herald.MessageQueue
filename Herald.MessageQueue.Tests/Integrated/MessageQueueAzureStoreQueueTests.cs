using Herald.MessageQueue.Tests.Helpers.AzureStorageQueue;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace Herald.MessageQueue.Tests.Integrated
{
    public class MessageQueueAzureStoreQueueTests : MessageQueueTestsBase
    {
        public MessageQueueAzureStoreQueueTests() : base(new AzureStorageQueueThreadSafeBuilder())
        {
        }
    }
}
