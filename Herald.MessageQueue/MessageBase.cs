using System;

using Newtonsoft.Json;

namespace Herald.MessageQueue
{
    public abstract class MessageBase
    {
        public MessageBase()
        {
            CreationDate = DateTime.UtcNow;
        }

        public object QueueData { get; set; }

        [JsonProperty]
        public DateTime CreationDate { get; private set; }
    }
}
