using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Banzai;
using Common.Logging;
using SlimMessageBus.Host.Config;
using SlimMessageBus.Host.Pipeline;

namespace SlimMessageBus.Host.PipelineStages
{
    class GetTopicStage : Step
    {
        private static readonly ILog Log = LogManager.GetLogger<MessageBusBase>();

        private IDictionary<Type, PublisherSettings> settings;
        public GetTopicStage(MessageBusSettings settings) : base(WellKnownStep.Publish)
        {
            this.settings = settings.Publishers.ToDictionary(x => x.MessageType); ;
        }

        protected override Task<NodeResultStatus> PerformExecuteAsync(IExecutionContext<PipelineContext> context)
        {
            var c = context.Subject;
            if (string.IsNullOrEmpty(c.Topic))
            {
                c.Topic = GetDefaultTopic(c.MessageType);
            }
            return Task.FromResult(NodeResultStatus.Succeeded);
        }


        protected virtual string GetDefaultTopic(Type messageType)
        {
            // when topic was not provided, lookup default topic from configuration
            var publisherSettings = GetPublisherSettings(messageType);
            return GetDefaultTopic(messageType, publisherSettings);
        }

        protected virtual string GetDefaultTopic(Type messageType, PublisherSettings publisherSettings)
        {
            var topic = publisherSettings.DefaultTopic;
            if (topic == null)
            {
                throw new PublishMessageBusException($"An attempt to produce message of type {messageType} without specifying topic, but there was no default topic configured. Double check your configuration.");
            }
            Log.DebugFormat("Applying default topic {0} for message type {1}", topic, messageType);
            return topic;
        }

        protected PublisherSettings GetPublisherSettings(Type messageType)
        {
            PublisherSettings publisherSettings;
            if (!settings.TryGetValue(messageType, out publisherSettings))
            {
                throw new PublishMessageBusException($"Message of type {messageType} was not registered as a supported publish message. Please check your MessageBus configuration and include this type.");
            }

            return publisherSettings;
        }
    }
}
