using System.Collections.Generic;
using System.Threading.Tasks;
using Banzai;

namespace SlimMessageBus.Host.Pipeline
{
    public class Step : Node<PipelineContext>
    {
        public List<Banzai.INode<PipelineContext>> Befores { get; set;  }
        public List<Banzai.INode<PipelineContext>> Afters { get; set; }

        public WellKnownStep Name { get; set; }
        public Step(WellKnownStep name)
        {
            Afters = new List<INode<PipelineContext>>();
            Befores = new List<INode<PipelineContext>>();
            this.Name = name;
        }

        protected override async void OnAfterExecute(IExecutionContext<PipelineContext> context)
        {
            foreach (var node in Afters)
            {
                await node.ExecuteAsync(context);
            }
        }

        protected override async void OnBeforeExecute(IExecutionContext<PipelineContext> context)
        {
            foreach (var node in Befores)
            {
                await node.ExecuteAsync(context);
            }
        }

        protected override Task<NodeResultStatus> PerformExecuteAsync(IExecutionContext<PipelineContext> context)
        {
            return Task.FromResult(NodeResultStatus.Succeeded);
        }
    }
}