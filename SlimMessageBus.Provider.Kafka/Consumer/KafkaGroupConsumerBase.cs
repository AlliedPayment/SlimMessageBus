using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Logging;
using RdKafka;

namespace SlimMessageBus.Provider.Kafka
{
    public abstract class KafkaGroupConsumerBase : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger<KafkaGroupConsumerBase>();

        public readonly KafkaMessageBus MessageBus;
        public readonly string Group;

        protected EventConsumer Consumer;

        protected KafkaGroupConsumerBase(KafkaMessageBus messageBus, string group, List<string> topics)
        {
            MessageBus = messageBus;
            Group = group;

            var config = new Config
            {
                GroupId = group,
                EnableAutoCommit = false
            };
            Consumer = new EventConsumer(config, MessageBus.KafkaSettings.BrokerList);
            Consumer.OnMessage += OnMessage;
            Consumer.OnEndReached += OnEndReached;

            if (Log.IsInfoEnabled)
            {
                Log.InfoFormat("Subscribing to topics {0}", string.Join(",", topics));
            }
            Consumer.Subscribe(topics);
        }

        public Task Commit(TopicPartitionOffset offset)
        {
            return Consumer.Commit(new List<TopicPartitionOffset> { offset });
        }

        protected abstract void OnEndReached(object sender, TopicPartitionOffset offset);
        protected abstract void OnMessage(object sender, Message msg);

        #region Implementation of IDisposable

        public virtual void Dispose()
        {
            // first stop the consumer, so that messages do not get consumed at this point
            Consumer?.Stop().Wait();

            // dispose the consumer
            if (Consumer != null)
            {
                Consumer.Dispose();
                Consumer = null;
            }
        }

        #endregion
    }
}