using System;
using System.Threading.Tasks;
using Banzai;
using SlimMessageBus.Host.Config;
using SlimMessageBus.Host.Pipeline;

namespace SlimMessageBus.Host.PipelineStages
{
    class SerializeRequestResponseMessage : Step
    {

        private IMessageSerializer serializer;
        private IMessageSerializer messageWithHeadersSerializer;
        private RequestResponseSettings requestResponseSettings;
        public SerializeRequestResponseMessage(IMessageSerializer serializer, IMessageSerializer messageWithHeadersSerializer, RequestResponseSettings requestResponseSettings) : base(WellKnownStep.Publish)
        {
            this.serializer = serializer;
            this.messageWithHeadersSerializer = messageWithHeadersSerializer;
            this.requestResponseSettings = requestResponseSettings;
        }

        public override Task<bool> ShouldExecuteAsync(IExecutionContext<PipelineContext> context)
        {
            return Task.FromResult(context.Subject.Intent == Intents.RequestResponse &&
                                   (context.Subject.Payload == null || context.Subject.Payload.Length == 0));
        }

        protected override Task<NodeResultStatus> PerformExecuteAsync(IExecutionContext<PipelineContext> context)
        {
            var subj = context.Subject;

            var replyTo = this.requestResponseSettings.Topic;
            //var replyTo = context.Subject.ReplyTo;
            var created = DateTime.UtcNow;
            var expires = created.Add(context.Subject.RequestTimeout.GetValueOrDefault(TimeSpan.FromMinutes(1)));
            var payload = SerializeRequest(subj.MessageType, subj.Message, subj.RequestId, replyTo, expires);
            
            subj.Payload = payload;

            return Task.FromResult(NodeResultStatus.Succeeded);
        }

        public virtual byte[] SerializeRequest(Type requestType, object request, string requestId, string replyTo, DateTimeOffset? expires)
        {
            var requestPayload = serializer.Serialize(requestType, request);

            // create the request wrapper message
            var requestMessage = new MessageWithHeaders(requestPayload);
            requestMessage.SetHeader(ReqRespMessageHeaders.RequestId, requestId);
            requestMessage.SetHeader(ReqRespMessageHeaders.ReplyTo, replyTo);
            if (expires.HasValue)
            {
                requestMessage.SetHeader(ReqRespMessageHeaders.Expires, expires.Value);
            }

            var requestMessagePayload = messageWithHeadersSerializer.Serialize(typeof(MessageWithHeaders), requestMessage);
            return requestMessagePayload;
        }
    }
}