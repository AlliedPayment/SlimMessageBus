using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Banzai;
using SlimMessageBus.Host.Config;
using SlimMessageBus.Host.Pipeline;

namespace SlimMessageBus.Host.PipelineStages
{
    class SerializeTransportMessageStage : Step
    {

        private IMessageSerializer serializer;
        public SerializeTransportMessageStage(IMessageSerializer serializer) : base(WellKnownStep.Publish)
        {
            this.serializer = serializer;
        }

        public override Task<bool> ShouldExecuteAsync(IExecutionContext<PipelineContext> context)
        {
            return Task.FromResult(context.Subject.Intent == Intents.Publish && 
                (context.Subject.Payload == null || context.Subject.Payload.Length == 0));
        }

        protected override Task<NodeResultStatus> PerformExecuteAsync(IExecutionContext<PipelineContext> context)
        {
            var subj = context.Subject;
            var payload = serializer.Serialize(subj.MessageType, subj.Message);
            subj.Payload = payload;
            return Task.FromResult(NodeResultStatus.Succeeded);
        }
    }
}
