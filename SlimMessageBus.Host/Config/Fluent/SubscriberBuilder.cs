using System;

namespace SlimMessageBus.Host.Config
{
    public class SubscriberBuilder<T> : ConsumerBuilder<T>
    {
        public SubscriberBuilder(MessageBusSettings settings)
            : base(settings)
        {
        }

        public TopicSubscriberBuilder<T> Topic(string topic)
        {
            return new TopicSubscriberBuilder<T>(topic, Settings);
        }

        public TopicSubscriberBuilder<T> Topic(string topic, Action<TopicSubscriberBuilder<T>> topicConfig)
        {
            var b = Topic(topic);
            topicConfig(b);
            return b;
        }
    }
}