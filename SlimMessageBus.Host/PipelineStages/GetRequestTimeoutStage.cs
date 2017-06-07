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
    class GetRequestTimeoutStage : Step
    {
        private MessageBusSettings settings;
        private static readonly ILog Log = LogManager.GetLogger<MessageBusBase>();
        private IDictionary<Type, PublisherSettings> publisherSettings;


        public GetRequestTimeoutStage(MessageBusSettings settings) : base(WellKnownStep.Publish)
        {
            this.settings = settings;
            this.publisherSettings = settings.Publishers.ToDictionary(x => x.MessageType); ;

        }

        protected virtual TimeSpan GetDefaultRequestTimeout(Type requestType, PublisherSettings publisherSettings)
        {
            var timeout = publisherSettings.Timeout ?? settings.RequestResponse.Timeout;
            Log.DebugFormat("Applying default timeout {0} for message type {1}", timeout, requestType);
            return timeout;
        }

        protected PublisherSettings GetPublisherSettings(Type messageType)
        {
            PublisherSettings p;
            if (!publisherSettings.TryGetValue(messageType, out p))
            {
                throw new PublishMessageBusException($"Message of type {messageType} was not registered as a supported publish message. Please check your MessageBus configuration and include this type.");
            }

            return p;
        }

        protected override Task<NodeResultStatus> PerformExecuteAsync(IExecutionContext<PipelineContext> context)
        {
            var c = context.Subject;
            if (!c.RequestTimeout.HasValue)
            {
                c.RequestTimeout = GetDefaultRequestTimeout(c.MessageType, GetPublisherSettings(c.MessageType));
            }
            return Task.FromResult(NodeResultStatus.Succeeded);
        }
    }
}
