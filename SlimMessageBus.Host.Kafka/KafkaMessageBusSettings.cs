using System.Collections;
using System.Collections.Generic;

namespace SlimMessageBus.Host.Kafka
{
    public class KafkaMessageBusSettings
    {
        public string BrokerList { get; set; }
        public Dictionary<string, string> ProducerSettings { get; set; }
        public Dictionary<string, string> TopicSettings { get; set; }
        public KafkaTopicProducerFactory TopicProducerFactory { get; set; }
        public KafkaProducerFactory KafkaProducerFactory { get; set; }
        public KafkaConsumerSettingsFactory KafkaConsumerSettingsFactory { get; set; }
        public Dictionary<string, string> ConsumerSettings { get; set; }

        public KafkaMessageBusSettings(string brokerList)
        {
            BrokerList = brokerList;
            KafkaConsumerSettingsFactory = new KafkaConsumerSettingsFactory();
            ProducerSettings = new Dictionary<string, string>();
            TopicSettings = new Dictionary<string, string>();
            ConsumerSettings = new Dictionary<string, string>();
            TopicProducerFactory = new KafkaTopicProducerFactory();
            KafkaProducerFactory = new KafkaProducerFactory();
        }
    }
}