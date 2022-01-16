using Newtonsoft.Json;

namespace Herald.MessageQueue
{
    public abstract class MessageBase
    {
        [JsonIgnore]
        public virtual object QueueData { get; set; }
    }
}
