using System;
using Common.Logging;
using RdKafka;

namespace SlimMessageBus.Host.Kafka
{
    public class KafkaTopicProducerFactory
    {
        public virtual KafkaTopicProducer Create(string name, Producer producer, KafkaMessageBusSettings settings)
        {
            var topicConfig = new TopicConfig();
            foreach (var kvp in settings.TopicSettings)
            {
                topicConfig[kvp.Key] = kvp.Value;
            }
            return new KafkaTopicProducer(name, producer, topicConfig);
        }
    }
    public class KafkaConsumerSettingsFactory
    {
        public virtual RdKafka.Config Create(RdKafka.Config cfg, KafkaMessageBusSettings settings)
        {
            foreach (var kvp in settings.ConsumerSettings)
            {
                cfg[kvp.Key] = kvp.Value;
            }
            return cfg;
        }
    }
    public class KafkaProducerFactory
    {
        public virtual Producer Create(string brokerList, KafkaMessageBusSettings settings)
        {
            var producerConfig = new RdKafka.Config();
            foreach (var kvp in settings.ProducerSettings)
            {
                producerConfig[kvp.Key] = kvp.Value;
            }
            return new Producer(producerConfig, brokerList);
        }
    }
    public class KafkaTopicProducer : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger<KafkaTopicProducer>();

        public string Name;
        public Topic Topic;

        public KafkaTopicProducer(string name, Producer producer, TopicConfig topicConfig = null)
        {
            Name = name;
            Topic = producer.Topic(name, topicConfig);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (Topic != null)
            {
                Topic.DisposeSilently("kafka topic", Log);
                Topic = null;
            }
        }

        #endregion
    }
}