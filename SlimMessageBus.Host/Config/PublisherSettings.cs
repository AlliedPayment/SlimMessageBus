using System;

namespace SlimMessageBus.Host.Config
{
    public class PublisherSettings
    {
        public Type MessageType { get; set; }
        public string DefaultTopic { get; set; }
        public TimeSpan? Timeout { get; set; }
        public Func<int, int, object> CustomPartitioner { get; set; }
    }
}