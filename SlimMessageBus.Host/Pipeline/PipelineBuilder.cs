using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banzai;
using SlimMessageBus.Host.Config;
using SlimMessageBus.Host.Pipeline.Stages;
using SlimMessageBus.Host.PipelineStages;

namespace SlimMessageBus.Host.Pipeline
{
    public class PipelineBuilder
    {

        private List<Step> Steps;

        public PipelineBuilder(Action<PipelineContext> publish, MessageBusSettings settings)
        {
            Steps = new List<Step>();
            Steps.Add(new Step(WellKnownStep.MutateOutgoingMessages));
            Steps.Add(new Step(WellKnownStep.MutateOutgoingTransportMessage));
            
            Steps.Add(new PipelineStages.GetTopicStage(settings));
            Steps.Add(new PipelineStages.GetRequestTimeoutStage(settings));
            Steps.Add(new PipelineStages.SerializeTransportMessageStage(settings.Serializer));
            Steps.Add(new SendInternalStage(settings));
            Steps.Add(new PipelineStages.SerializeRequestResponseMessage(settings.Serializer, settings.MessageWithHeadersSerializer));
            Steps.Add(new PublishStage(publish));
        }
        public IPipeline Build()
        {
            var n = new Banzai.PipelineNode<PipelineContext>();
            foreach (var step in Steps)
            {
                n.AddChild(step);
            }
            
            return new Pipeline(n);
        }

        public void Register(WellKnownStep step, Action<PipelineContext> action)
        {
            var s = Steps.FirstOrDefault(x => x.Name == step);
            if (s == null) return;
            var status = NodeResultStatus.Succeeded;
            var task = new Task<NodeResultStatus>(() => status);
            s.Afters.Add(new FuncNode<PipelineContext>()
            {
                ExecutedFunc = (x) =>
                {
                    action.Invoke(x.Subject);
                    return Task.FromResult( NodeResultStatus.Succeeded);
                }
            }); 
        }
    }
}
