using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Banzai;
using Common.Logging;
using SlimMessageBus.Host.Config;
using SlimMessageBus.Host.Pipeline;

namespace SlimMessageBus.Host.PipelineStages
{
    class SendInternalStage : Step 
    {
        private static readonly ILog Log = LogManager.GetLogger<MessageBusBase>();
        private MessageBusSettings settings;
        private IDictionary<Type, PublisherSettings> publishers;
        

        
        public SendInternalStage(MessageBusSettings settings) : base(WellKnownStep.Publish)
        {
            this.settings = settings;
            this.publishers = settings.Publishers.ToDictionary(x => x.MessageType); ;

            
        }

        protected virtual string GenerateRequestId()
        {
            return Guid.NewGuid().ToString("N");
        }

        public override Task<bool> ShouldExecuteAsync(IExecutionContext<PipelineContext> context)
        {
            return Task.FromResult(context.Subject.Intent == Intents.RequestResponse);
        }

        protected override Task<NodeResultStatus> PerformExecuteAsync(IExecutionContext<PipelineContext> context)

        {
            if (settings.RequestResponse == null)
            {
                throw new PublishMessageBusException("An attempt to send request when request/response communication was not configured for the message bus. Ensure you configure the bus properly before the application starts.");
            }

            //return await Task.FromCanceled<TResponseMessage>(cancellationToken);

            // check if the cancellation was already requested
            context.Subject.CancellationToken.ThrowIfCancellationRequested();
            var request = context.Subject.Request;
            var requestType = request.GetType();
     
            var created = DateTime.UtcNow;
            var expires = created.Add(context.Subject.RequestTimeout.GetValueOrDefault(TimeSpan.FromMinutes(1)));

            // generate the request guid
            context.Subject.RequestId = GenerateRequestId();
        
            // record the request state
            context.Subject.RequestState = new PendingRequestState(context.Subject.RequestId, request, requestType, context.Subject.ResponseMessageType, created, expires, context.Subject.CancellationToken);
            context.Subject.PendingRequestStore.Add(context.Subject.RequestState);

            if (Log.IsDebugEnabled)
                Log.DebugFormat("Added to PendingRequests, total is {0}", context.Subject.PendingRequestStore.GetCount());
            return Task.FromResult(NodeResultStatus.Succeeded);

        }

   
       
      
    
    }
}
