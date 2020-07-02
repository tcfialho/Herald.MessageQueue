using Newtonsoft.Json;

using System;

namespace Herald.MessageQueue
{
    public abstract class MessageBase
    {
        [JsonIgnore]
        public virtual object QueueData { get; set; }
    }
}
