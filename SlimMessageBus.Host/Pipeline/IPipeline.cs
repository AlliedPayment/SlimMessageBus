using System;
using System.Threading.Tasks;

namespace SlimMessageBus.Host.Pipeline
{
    public interface IPipeline
    {
        Task<PipelineContext> Send(PipelineContext context);
        void SetPublish(Action<PipelineContext> publish);
    }

    public class Pipeline : IPipeline
    {
        private Banzai.INode<PipelineContext> pipe;
        public Pipeline(Banzai.INode<PipelineContext> pipe)
        {
            this.pipe = pipe;
        }
        public Task<PipelineContext> Send(PipelineContext context)
        {
            return pipe.ExecuteAsync(context)
                .ContinueWith(x => x.Result.GetSubjectAs<PipelineContext>());
        }

        public void SetPublish(Action<PipelineContext> publish)
        {
            throw new NotImplementedException();
        }
    }

}
