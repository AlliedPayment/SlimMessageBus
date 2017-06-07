using System;
using System.Threading.Tasks;
using Banzai;
using Common.Logging;

namespace SlimMessageBus.Host.Pipeline.Stages
{
    class PublishStage : Step
    {
        private Action<PipelineContext> publish;
        private static readonly ILog Log = LogManager.GetLogger<MessageBusBase>();

        public PublishStage(Action<PipelineContext> publish) : base(WellKnownStep.Publish)
        {
            this.publish = publish;
        }

        protected override Task<NodeResultStatus> PerformExecuteAsync(IExecutionContext<PipelineContext> context)
        {
            try
            {
                publish.Invoke(context.Subject);
                return Task.FromResult(NodeResultStatus.Succeeded);
            }
            catch (Exception e)
            {
                Log.Error("PublishStep",e);
                return Task.FromResult(NodeResultStatus.Failed);
            }
        }
    }
}
